namespace Vaflov {
    public abstract class GameEvent3Base<V, T, U, W> : GameEventBase where V : GameEvent3Base<V, T, U, W> {
        public abstract void Raise(T arg1, U arg2, W arg3);

        public virtual void AddListener(GameEventListener3Base<V, T, U, W> listener) {
            #if UNITY_EDITOR
            base.AddListener(listener);
            #endif
        }

        public virtual void RemoveListener(GameEventListener3Base<V, T, U, W> listener) {
            #if UNITY_EDITOR
            base.RemoveListener(listener);
            #endif
        }
    }
}