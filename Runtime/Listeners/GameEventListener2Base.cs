﻿using ExtEvents;
using UnityEngine;

namespace Vaflov {
    public abstract class GameEventListener2Base<V, T, U> : GameEventListenerBase where V : GameEvent2Base<V, T, U> {
        [HideInInspector]
        public V eventRef;
        public abstract ExtEvent<T, U> Response { get; }

        public void CallResponse(T arg1, U arg2) {
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
