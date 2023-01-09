using Microsoft.CSharp;
using Sirenix.OdinInspector;
using System.CodeDom;
using UnityEngine;

namespace Vaflov {
    public abstract class GameEvent3Base<V, T, U, W> : GameEventBase where V : GameEvent3Base<V, T, U, W> {
        public abstract void Raise(T arg1, U arg2, W arg3);

        public virtual void AddListener(GameEventListener3Base<V, T, U, W> listener) {
            #if UNITY_EDITOR
            listeners.Add(listener);
            #endif
        }

        public virtual void RemoveListener(GameEventListener3Base<V, T, U, W> listener) {
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