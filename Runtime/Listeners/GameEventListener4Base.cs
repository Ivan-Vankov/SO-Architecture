using ExtEvents;
using UnityEngine;

namespace Vaflov {
    public abstract class GameEventListener4Base<V, T1, T2, T3, T4> : GameEventListenerBase where V : GameEvent4Base<V, T1, T2, T3, T4> {
        [HideInInspector]
        public V eventRef;
        public abstract ExtEvent<T1, T2, T3, T4> Response { get; }

        public void CallResponse(T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            Response?.Invoke(arg1, arg2, arg3, arg4);
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
