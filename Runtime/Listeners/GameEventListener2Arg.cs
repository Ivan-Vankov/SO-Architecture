using System;
using UnityEngine;
using UnityEngine.Events;

namespace Vaflov {
    public class GameEventListener2Arg<T, U> : MonoBehaviour {

        public GameEvent2Arg<T, U> eventRef;
        public Action<T, U> actionResponse;
        public UnityEvent<T, U> response;

        public void CallResponse(T arg1, U arg2) {
            response?.Invoke(arg1, arg2);
            actionResponse?.Invoke(arg1, arg2);
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
    }
}