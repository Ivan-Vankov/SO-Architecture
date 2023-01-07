using ExtEvents;
using UnityEngine;

namespace Vaflov {
    public abstract class GameEventListener1Base<V, T> : GameEventListenerBase where V : GameEvent1Base<V, T> {
        [HideInInspector]
        public V eventRef;
        public abstract ExtEvent<T> Response { get; }

        public void CallResponse(T arg1) {
            Response?.Invoke(arg1);
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
