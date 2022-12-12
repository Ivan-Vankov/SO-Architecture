using System;
using UnityEngine;

namespace Vaflov {
    public class GameEvent3Arg<T, U, V> : GameEventBase {

        public Action<T, U, V> action;

        public void Raise(T arg1, U arg2, V arg3) {
            action?.Invoke(arg1, arg2, arg3);
        }
    }
}
