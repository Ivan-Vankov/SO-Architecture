#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using ExtEvents;
using UnityEngine;

namespace Vaflov {
    public class GameEventListener1Arg<T> : GameEventListenerBase {
        [HideInInspector]
        public GameEvent1Arg<T> eventRef;
        public ExtEvent<T> response;

        public void CallResponse(T arg1) {
            response?.Invoke(arg1);
        }

        public void OnEnable() {
            if (eventRef) {
                eventRef.action += CallResponse;
            }
        }

        public void OnDisable() {
            if (eventRef) {
                eventRef.action -= CallResponse;
            }
        }

        #if UNITY_EDITOR && ODIN_INSPECTOR
        public override void AssignGameEvent() {
            eventRef = (GameEvent1Arg<T>)gameEvent;
        }
        #endif
    }
}