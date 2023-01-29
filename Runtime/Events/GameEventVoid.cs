#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System;

namespace Vaflov {
    public class GameEventVoid : GameEventBase {
        public Action action;

        #if ODIN_INSPECTOR
        [Button]
        #endif
        public void Raise() {
            action?.Invoke();
        }

        public void AddListener(GameEventListenerVoid listener) {
            #if UNITY_EDITOR
            base.AddListener(listener);
            #endif
            action -= listener.CallResponse;
            action += listener.CallResponse;
        }

        public void RemoveListener(GameEventListenerVoid listener) {
            #if UNITY_EDITOR
            base.RemoveListener(listener);
            #endif
            action -= listener.CallResponse;
        }
    }
}