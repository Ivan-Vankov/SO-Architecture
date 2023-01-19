#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
#endif
#endif
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Vaflov {
    [HideInPlayMode]
    [HideLabel]
    [Serializable]
    public class GameEventEditMenu { }

    [Serializable]
    public class ObjSet<T> : ISerializationCallbackReceiver where T : UnityEngine.Object {
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

        // Lists can be serialized natively by unity => no custom serialization needed
        public void OnBeforeSerialize() { }

        // Fill dictionary with list contents
        public void OnAfterDeserialize() {
            objToIdxs.Clear();
            for (int i = 0; i < objects.Count; ++i) {
                objToIdxs.Add(objects[i], i);
            }
        }

        public void Add(T listener) {
            //if (!objects.Contains(listener))
            //    objects.Add(listener);
            if (!objToIdxs.ContainsKey(listener)) {
                objects.Add(listener);
                objToIdxs[listener] = objects.Count - 1;
            }
        }

        public void Remove(T obj) {
            //objects.Remove(obj);
            if (!objToIdxs.Remove(obj, out int idx))
                return;
            var lastObj = objects[objects.Count - 1];
            objects[idx] = lastObj;
            objects.RemoveAt(objects.Count - 1);
            if (idx < objects.Count) {
                objToIdxs[lastObj] = idx;
            }
        }

        public int Count => objects.Count;
    }

    public class GameEventBase : EditorScriptableObject<GameEventBase> {
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
            if (listener.parent is MonoBehaviour monoBehaviour) {
                componentListeners.Remove(monoBehaviour);
            } else if (listener.parent is ScriptableObject scriptableObject) {
                SOListeners.Remove(scriptableObject);
            } else {
                Debug.Assert(false, "Unknown game event listener object");
            }
        }

        [PropertyOrder(40)]
        public GameEventEditMenu editMenu;
    }
}
