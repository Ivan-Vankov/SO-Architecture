#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static Vaflov.Config;

namespace Vaflov {
    [DefaultExecutionOrder(-2000)]
    public class GameEventListener : MonoBehaviour {
        #if ODIN_INSPECTOR
        [AssetsOnly]
        [OnValueChanged(nameof(RefreshListener))]
        [Required]
        #endif
        public GameEventBase eventRef;

        [HideInInspector]
        public GameEventListenerBase listener;

        #if ODIN_INSPECTOR
        [PropertyOrder(10)]
        [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        #endif
        public GameEventListenerBase Listener {
            get {
                if (listener)
                    listener.parent = this;
                return listener;
            }
            set {
                listener = value;
                if (listener)
                    listener.parent = this;
            }
        }

        public void RefreshListener() {
            Listener = GameEventListenerUtil.GetListenerInstance(eventRef);
            if (Listener) {
                Listener.AssignGameEvent(eventRef);
            }
        }

        private void OnEnable() {
            var listener = Listener;
            if (listener) {
                listener.OnInit();
            }
        }

        private void OnDisable() {
            var listener = Listener;
            if (listener) {
                listener.OnDone();
            }
        }
    }
}
