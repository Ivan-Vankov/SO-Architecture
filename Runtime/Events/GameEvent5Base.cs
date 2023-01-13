namespace Vaflov {
    public abstract class GameEvent5Base<V, T1, T2, T3, T4, T5> : GameEventBase where V : GameEvent5Base<V, T1, T2, T3, T4, T5> {
        public abstract void Raise(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

        public virtual void AddListener(GameEventListener5Base<V, T1, T2, T3, T4, T5> listener) {
            #if UNITY_EDITOR
            base.AddListener(listener);
            #endif
        }

        public virtual void RemoveListener(GameEventListener5Base<V, T1, T2, T3, T4, T5> listener) {
            #if UNITY_EDITOR
            base.RemoveListener(listener);
            #endif
        }
    }
}