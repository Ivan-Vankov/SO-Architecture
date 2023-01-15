using ExtEvents;
using UnityEngine;

namespace Vaflov {
    public class GameEventListenerVoid : GameEventListenerBase {
        [HideInInspector]
        public GameEventVoid eventRef;
        public ExtEvent response;

        public void CallResponse() {
            response?.Invoke();
        }

        public override void OnInit() {
            if (eventRef) {
                eventRef.AddListener(this);
            }
        }

        public override void OnDone() {
            if (eventRef) {
                eventRef.RemoveListener(this);
            }
        }

        public override void AssignGameEvent(GameEventBase gameEvent) {
            eventRef = (GameEventVoid)gameEvent;
        }
    }
}