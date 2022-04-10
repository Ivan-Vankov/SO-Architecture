using System;
using UnityEngine;

namespace Vaflov {
    public class GameEvent1Arg<T> : ScriptableObject {

        public Action<T> action;

        public void Raise(T arg1) {
            action?.Invoke(arg1);
        }
    }
}