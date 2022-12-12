using ExtEvents;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Vaflov {
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

        public override void AssignGameEvent() {
            eventRef = (GameEventVoid)gameEvent;
        }
    }
}