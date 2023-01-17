#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
#endif
#endif
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using static Vaflov.Config;
using System.Runtime.CompilerServices;

namespace Vaflov {
    public static class GameEventEditorEvents {
        public static Action OnGameEventEditorPropChanged;
        public static Action<ScriptableObject> OnGameEventDuplicated;
    }

    [HideInPlayMode]
    [HideLabel]
    [Serializable]
    public class GameEventEditMenu { }

    [Serializable]
    public class ObjSet<T> where T: UnityEngine.Object {
        [HideInInspector]
        public string name = "Objects";

        //[ReadOnly]
        [LabelText("$" + nameof(name))]
        [ListDrawerSettings(
            DraggableItems = false,
            //Expanded = true,
            ShowPaging = true,
            NumberOfItemsPerPage = 10
            //ShowItemCount = false
            //HideRemoveButton = true
            )]
        public List<T> objects = new List<T>();
        public Dictionary<T, int> objToIdxs = new Dictionary<T, int>();

        public void Add(T listener) {
            if (!objToIdxs.ContainsKey(listener)) {
                objects.Add(listener);
                objToIdxs[listener] = objects.Count - 1;
            }
        }

        public void Remove(T obj) {
            if (!objToIdxs.ContainsKey(obj))
                return;
            var idx = objToIdxs[obj];
            objects[idx] = objects[objects.Count - 1];
            objects.RemoveAt(objects.Count - 1);
            if (idx < objects.Count) {
                objToIdxs[objects[idx]] = idx;
            }
        }

        public int Count => objects.Count;
    }

    public class GameEventBase : ScriptableObject, ISortKeyObject, IEditorObject {
        [HideInInspector] public EditorObject editorObj = null;
        [ShowInInspector, HideLabel, HideReferenceObjectPicker, DisableContextMenu]
        public EditorObject EditorObject {
            get {
                //if (editorObj == null || editorObj.editorObjParent == null) {
                //    editorObj = new EditorObject(this, typeof(GameEventBase), GameEventEditorEvents.OnGameEventEditorPropChanged);
                //}
                editorObj ??= new EditorObject();
                editorObj.Init(this, typeof(Constant<>), ConstantEditorEvents.OnConstantEditorPropChanged);
                return editorObj;
            }
            set => Debug.Assert(true, "Editing editor object internals");
        }

        public string EditorGroup { get => EditorObject.editorGroup; set => EditorObject.editorGroup = value; }
        public int SortKey { get => EditorObject.sortKey; set => EditorObject.sortKey = value; }
        public string EditorComment { get => EditorObject.editorComment; set => EditorObject.editorComment = value; }

        [HideLabel]
        [FoldoutGroup("Listeners", true)]
        [PropertyOrder(30)]
        public ObjSet<Component> componentListeners = new ObjSet<Component>() { name = "Component Listeners" };

        [HideLabel]
        [FoldoutGroup("Listeners", true)]
        [PropertyOrder(31)]
        public ObjSet<ScriptableObject> SOListeners = new ObjSet<ScriptableObject>() { name = "SO Listeners" };

        public void AddListener(GameEventListenerBase listener) {
            if (listener.parent is MonoBehaviour) {
                componentListeners.Add(Unsafe.As<MonoBehaviour>(listener.parent));
            } else {
                SOListeners.Add(Unsafe.As<ScriptableObject>(listener.parent));
            }
        }

        public void RemoveListener(GameEventListenerBase listener) {
            if (listener.parent is MonoBehaviour) {
                componentListeners.Remove(Unsafe.As<MonoBehaviour>(listener.parent));
            } else {
                SOListeners.Remove(Unsafe.As<ScriptableObject>(listener.parent));
            }
        }

        public virtual Texture GetEditorIcon() => null;

        [PropertyOrder(40)]
        public GameEventEditMenu editMenu;

        public virtual string EditorToString() => null;

        public virtual List<OdinContextMenuItem> GetContextMenuItems() {
            return OdinContextMenuItem.GetDefaultContextMenuItems(this, GameEventEditorEvents.OnGameEventDuplicated);
        }
    }
}
