#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static Vaflov.Config;

namespace Vaflov {
    public class GameEventListenerSO : EditorScriptableObject {
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
                #if UNITY_EDITOR
                if (listener) {
                    DestroyImmediate(listener, true);
                    AssetDatabase.SaveAssets();
                }
                #endif
                eventRef = value;
                #if UNITY_EDITOR
                listener = GameEventListenerUtil.GetListenerInstance(eventRef);
                if (listener) {
                    listener.parent = this;
                    listener.AssignGameEvent(eventRef);
                    listener.name = "Listener";
                    AssetDatabase.AddObjectToAsset(listener, this);
                    AssetDatabase.SaveAssets();
                }
                #endif
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

        private void OnDestroy() {
            if (listener) {
                DestroyImmediate(listener, true);
            }
        }
    }

    #if UNITY_EDITOR
    public class DescriptorDeleteDetector : AssetModificationProcessor {
        static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions _) {
            var listenerSO = AssetDatabase.LoadAssetAtPath<GameEventListenerSO>(path);
            if (listenerSO) {
                // Unity doesn't call OnDisable/Destroy on scriptable objects that are deleted for some reason ;(
                if (listenerSO.listener) {
                    listenerSO.listener.OnDone();
                    Object.DestroyImmediate(listenerSO.listener, true);
                }
            }
            return AssetDeleteResult.DidNotDelete;
        }
    }
    #endif
}
