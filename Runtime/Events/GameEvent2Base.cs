using Microsoft.CSharp;
using Sirenix.OdinInspector;
using System;
using System.CodeDom;
using UnityEngine;
using static Vaflov.Config;

namespace Vaflov {
    public abstract class GameEvent2Base<V, T, U> : GameEventBase where V : GameEvent2Base<V, T, U> {
        public abstract void Raise(T arg1, U arg2);

        public virtual void AddListener(GameEventListener2Base<V, T, U> listener) {
            #if UNITY_EDITOR
            listeners.Add(listener);
            #endif
        }

        public virtual void RemoveListener(GameEventListener2Base<V, T, U> listener) {
            #if UNITY_EDITOR
            var index = listeners.IndexOf(listener);
            if (index > -1) {
                listeners[index] = listeners[listeners.Count - 1];
                listeners.RemoveAt(listeners.Count - 1);
            }
            #endif
        }
    }
}
