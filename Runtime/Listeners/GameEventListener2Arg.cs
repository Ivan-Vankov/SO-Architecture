using ExtEvents;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Vaflov {
    public class GameEventListener2Arg<T, U> : GameEventListenerBase {
        [HideInInspector]
        public GameEvent2Arg<T, U> eventRef;
        public ExtEvent<T, U> response;

        public void CallResponse(T arg1, U arg2) {
            response?.Invoke(arg1, arg2);
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

        public override void AssignGameEvent() {
            eventRef = (GameEvent2Arg<T, U>)gameEvent;
        }
    }
}