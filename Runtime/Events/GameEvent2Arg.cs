using System;
using UnityEngine;

namespace Vaflov {
    public class GameEvent2Arg<T, U> : ScriptableObject {

        public Action<T, U> action;

        public void Raise(T arg1, U arg2) {
            action?.Invoke(arg1, arg2);
        }
    }
}