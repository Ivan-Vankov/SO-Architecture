using ExtEvents;
using UnityEngine;

namespace Vaflov {
    public abstract class GameEventListener2Base<T, U, V> : GameEventListenerBase where V : GameEvent2Base<T, U, V> {
        [HideInInspector]
        public abstract V EventRef { get; set; }
        public abstract ExtEvent<T, U> Response { get; }

        public void CallResponse(T arg1, U arg2) {
            Response?.Invoke(arg1, arg2);
        }

        public void OnEnable() {
            if (EventRef) {
                EventRef.AddListener(this);
                //EventRef.Subscribe(CallResponse);
                //EventRef.action += CallResponse;
            }
        }

        public void OnDisable() {
            if (EventRef) {
                EventRef.RemoveListener(this);
                //EventRef.Unsubscribe(CallResponse);
                //EventRef.action -= CallResponse;
            }
        }

        #if UNITY_EDITOR
        public override void AssignGameEvent() {
            EventRef = (V)gameEvent;
        }
        #endif
    }
}
