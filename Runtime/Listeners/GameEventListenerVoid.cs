using ExtEvents;
using UnityEngine;

namespace Vaflov {
    [AddComponentMenu("")]
    public class GameEventListenerVoid : GameEventListenerBase {
        [HideInInspector]
        public GameEventVoid eventRef;
        public ExtEvent response;

        public void CallResponse() {
            response?.Invoke();
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
            eventRef = (GameEventVoid)gameEvent;
        }
        #endif
    }
}