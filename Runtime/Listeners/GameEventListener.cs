using System;
using UnityEngine;
using UnityEngine.Events;

namespace Vaflov {
    public class GameEventListener : MonoBehaviour {

        public GameEvent eventRef;
        public Action actionResponse; 
        public UnityEvent response;

        public void CallResponse() {
            response?.Invoke();
            actionResponse?.Invoke();
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