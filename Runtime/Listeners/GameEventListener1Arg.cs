using ExtEvents;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Vaflov {
    public class GameEventListener1Arg<T> : GameEventListenerBase {
        [HideInInspector]
        public GameEvent1Arg<T> eventRef;
        public ExtEvent<T> response;

        public void CallResponse(T arg1) {
            response?.Invoke(arg1);
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
            eventRef = (GameEvent1Arg<T>)gameEvent;
        }
    }
}