using System;
using UnityEngine;
using UnityEngine.Events;

namespace Vaflov {
    public class GameEventListener1Arg<T> : MonoBehaviour {

        public GameEvent1Arg<T> eventRef;
        public Action<T> actionResponse;
        public UnityEvent<T> response;

        public void CallResponse(T arg1) {
            response?.Invoke(arg1);
            actionResponse?.Invoke(arg1);
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