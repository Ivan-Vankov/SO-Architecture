using System;
using UnityEngine;

namespace Vaflov {
    public abstract class GameEvent2Base<T, U> : GameEventBase {
        public abstract void Raise(T arg1, U arg2);
    }
}
