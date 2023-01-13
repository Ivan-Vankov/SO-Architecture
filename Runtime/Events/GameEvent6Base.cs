namespace Vaflov {
    public abstract class GameEvent6Base<V, T1, T2, T3, T4, T5, T6> : GameEventBase where V : GameEvent6Base<V, T1, T2, T3, T4, T5, T6> {
        public abstract void Raise(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);

        public virtual void AddListener(GameEventListener6Base<V, T1, T2, T3, T4, T5, T6> listener) {
            #if UNITY_EDITOR
            base.AddListener(listener);
            #endif
        }

        public virtual void RemoveListener(GameEventListener6Base<V, T1, T2, T3, T4, T5, T6> listener) {
            #if UNITY_EDITOR
            base.RemoveListener(listener);
            #endif
        }
    }
}