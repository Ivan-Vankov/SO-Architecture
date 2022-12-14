#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using ExtEvents;
using UnityEngine;

namespace Vaflov {
    public class GameEventListener3Arg<T, U, V> : GameEventListenerBase {
        [HideInInspector]
        public GameEvent3Arg<T, U, V> eventRef;
        public ExtEvent<T, U, V> response;

        public void CallResponse(T arg1, U arg2, V arg3) {
            response?.Invoke(arg1, arg2, arg3);
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
            eventRef = (GameEvent3Arg<T, U, V>)gameEvent;
        }
        #endif
    }
}