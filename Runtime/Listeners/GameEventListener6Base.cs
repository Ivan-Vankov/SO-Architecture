using ExtEvents;
using UnityEngine;

namespace Vaflov {
    public abstract class GameEventListener6Base<V, T1, T2, T3, T4, T5, T6> : GameEventListenerBase where V : GameEvent6Base<V, T1, T2, T3, T4, T5, T6> {
        [HideInInspector]
        public V eventRef;
        public abstract ExtEvent<T1, T2, T3, T4, T5, T6> Response { get; }

        public void CallResponse(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
            Response?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
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
