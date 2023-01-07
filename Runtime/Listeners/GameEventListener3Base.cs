using ExtEvents;
using UnityEngine;

namespace Vaflov {
    public abstract class GameEventListener3Base<V, T, U, W> : GameEventListenerBase where V : GameEvent3Base<V, T, U, W> {
        [HideInInspector]
        public V eventRef;
        public abstract ExtEvent<T, U, W> Response { get; }

        public void CallResponse(T arg1, U arg2, W arg3) {
            Response?.Invoke(arg1, arg2, arg3);
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
