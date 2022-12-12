using System;

namespace Vaflov {
    public class GameEvent1Arg<T> : GameEventBase {
        public Action<T> action;

        public void Raise(T arg1) {
            action?.Invoke(arg1);
        }
    }
}