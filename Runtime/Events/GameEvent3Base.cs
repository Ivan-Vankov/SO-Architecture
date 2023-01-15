namespace Vaflov {
    public abstract class GameEvent3Base<V, T1, T2, T3> : GameEventBase where V : GameEvent3Base<V, T1, T2, T3> {
        public abstract void Raise(T1 arg1, T2 arg2, T3 arg3);

        public virtual void AddListener(GameEventListener<V, T1, T2, T3> listener) {
            #if UNITY_EDITOR
            base.AddListener(listener);
            #endif
        }

        public virtual void RemoveListener(GameEventListener<V, T1, T2, T3> listener) {
            #if UNITY_EDITOR
            base.RemoveListener(listener);
            #endif
        }
    }
}