#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Vaflov {
    [CreateAssetMenu(
        fileName = "Game Event Listener",
        menuName = "SO Architecture/Game Event Listener",
        order = 30)]
    [DefaultExecutionOrder(-2000)]
    public class GameEventListenerSO : ScriptableObject {
        [HideInInspector]
        public GameEventBase eventRef;

        #if ODIN_INSPECTOR
        [AssetsOnly]
        [ShowInInspector]
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

        private void OnEnable() {
            Debug.Log("Enable");
            if (listener) {
                Debug.Log("Init");
                listener.OnInit();
            }
        }

        private void OnDisable() {
            Debug.Log("Disable");
            if (listener) {
                Debug.Log("Done");
                listener.OnDone();
            }
        }
    }
}
