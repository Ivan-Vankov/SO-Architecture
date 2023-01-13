namespace Vaflov {
    public abstract class GameEvent4Base<V, T1, T2, T3, T4> : GameEventBase where V : GameEvent4Base<V, T1, T2, T3, T4> {
        public abstract void Raise(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

        public virtual void AddListener(GameEventListener4Base<V, T1, T2, T3, T4> listener) {
            #if UNITY_EDITOR
            base.AddListener(listener);
            #endif
        }

        public virtual void RemoveListener(GameEventListener4Base<V, T1, T2, T3, T4> listener) {
            #if UNITY_EDITOR
            base.RemoveListener(listener);
            #endif
        }
    }
}