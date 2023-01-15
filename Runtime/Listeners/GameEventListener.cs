#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Vaflov {
    [DefaultExecutionOrder(-2000)]
    public class GameEventListener : MonoBehaviour {
        #if ODIN_INSPECTOR
        [AssetsOnly]
        [OnValueChanged(nameof(RefreshListener))]
        [Required]
        public GameEventBase eventRef;
        #endif

        #if ODIN_INSPECTOR
        [PropertyOrder(10)]
        [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        public GameEventListenerBase listener;
        #endif

        public void RefreshListener() {
            listener = GetListenerInstance();
            if (listener) {
                listener.parent = this;
                listener.AssignGameEvent(eventRef);
            }
        }

        public GameEventListenerBase GetListenerInstance() {
            return GameEventListenerUtil.GetListenerInstance(eventRef);
        }

        private void OnEnable() {
            if (listener) {
                listener.OnInit();
            }
        }

        private void OnDisable() {
            if (listener) {
                listener.OnDone();
            }
        }
    }
}
