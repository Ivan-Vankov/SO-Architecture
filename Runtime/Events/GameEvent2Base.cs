namespace Vaflov {
    public abstract class GameEvent2Base<V, T, U> : GameEventBase where V : GameEvent2Base<V, T, U> {
        public abstract void Raise(T arg1, U arg2);

        public virtual void AddListener(GameEventListener2Base<V, T, U> listener) {
            #if UNITY_EDITOR
            base.AddListener(listener);
            #endif
        }

        public virtual void RemoveListener(GameEventListener2Base<V, T, U> listener) {
            #if UNITY_EDITOR
            base.RemoveListener(listener);
            #endif
        }
    }
}
