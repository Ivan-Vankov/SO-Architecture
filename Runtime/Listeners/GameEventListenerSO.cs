using System;
using System.Collections.Generic;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static Vaflov.Config;

namespace Vaflov {
    public static class GameEventListenerSOEditorEvents {
        public static Action OnGameEventListenerSOPropChanged;
        public static Action<ScriptableObject> OnGameEventListenerSODuplicated;
    }

    [CreateAssetMenu(
        fileName = "Game Event Listener",
        menuName = "SO Architecture/Game Event Listener",
        order = 30)]
    [DefaultExecutionOrder(-2000)]
    public class GameEventListenerSO : ScriptableObject, ISortKeyObject, IEditorObject {
        [HideInInspector] public EditorObject editorObj = null;
        [ShowInInspector, HideLabel, HideReferenceObjectPicker, DisableContextMenu]
        public EditorObject EditorObject {
            get {
                editorObj ??= new EditorObject();
                editorObj.Init(this, typeof(GameEventListenerSO), GameEventListenerSOEditorEvents.OnGameEventListenerSOPropChanged);
                return editorObj;
            }
            set => Debug.Assert(true, "Editing editor object internals");
        }

        public string EditorGroup { get => EditorObject.editorGroup; set => EditorObject.editorGroup = value; }
        public int SortKey { get => EditorObject.sortKey; set => EditorObject.sortKey = value; }
        public string EditorComment { get => EditorObject.editorComment; set => EditorObject.editorComment = value; }

        public Texture GetEditorIcon() => null;

        [HideInInspector]
        public GameEventBase eventRef;

        #if ODIN_INSPECTOR
        [AssetsOnly]
        [ShowInInspector]
        [LabelWidth(preferedEditorLabelWidth)]
        [Required]
        #endif
        public GameEventBase EventRef {
            get => eventRef;
            set {
                if (listener) {
                    DestroyImmediate(listener, true);
                    AssetDatabase.SaveAssets();
                }
                eventRef = value;
                listener = GameEventListenerUtil.GetListenerInstance(eventRef);
                if (listener) {
                    listener.parent = this;
                    listener.AssignGameEvent(eventRef);
                    listener.name = "Listener";
                    AssetDatabase.AddObjectToAsset(listener, this);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        #if ODIN_INSPECTOR
        [PropertyOrder(10)]
        [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        #endif
        public GameEventListenerBase listener;

        public void OnEnable() {
            //Debug.Log("Enable");
            if (listener) {
                //Debug.Log("Init");
                listener.OnInit();
            }
        }

        public void OnDisable() {
            //Debug.Log("Disable");
            if (listener) {
                //Debug.Log("Done");
                listener.OnDone();
            }
        }

        public string EditorToString() {
            return null;
        }

        public List<OdinContextMenuItem> GetContextMenuItems() {
            return editorObj?.GetDefaultContextMenuItems(GameEventListenerSOEditorEvents.OnGameEventListenerSODuplicated);
        }
    }

    #if UNITY_EDITOR
    public class DescriptorDeleteDetector : AssetModificationProcessor {
        static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions _) {
            var listenerSO = AssetDatabase.LoadAssetAtPath<GameEventListenerSO>(path);
            if (listenerSO) {
                // Unity doesn't call destroy on scriptable objects that are deleted for some reason ;(
                listenerSO.OnDisable();
            }
            return AssetDeleteResult.DidNotDelete;
        }
    }
    #endif
}
