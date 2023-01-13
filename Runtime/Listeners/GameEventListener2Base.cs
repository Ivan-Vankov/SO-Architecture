using ExtEvents;
using UnityEngine;

namespace Vaflov {
    public abstract class GameEventListener2Base<V, T1, T2> : GameEventListenerBase where V : GameEvent2Base<V, T1, T2> {
        [HideInInspector]
        public V eventRef;
        public abstract ExtEvent<T1, T2> Response { get; }

        public void CallResponse(T1 arg1, T2 arg2) {
            Response?.Invoke(arg1, arg2);
        }

        public void OnEnable() {
            if (eventRef) {
                eventRef.AddListener(this);
            }
        }

        public void OnDisable() {
            if (eventRef) {
                eventRef.RemoveListener(this);
            }
        }

        #if UNITY_EDITOR
        public override void AssignGameEvent() {
            eventRef = (V)gameEvent;
        }
        #endif
    }
}
