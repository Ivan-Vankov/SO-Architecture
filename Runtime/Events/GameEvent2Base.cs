namespace Vaflov {
    public abstract class GameEvent2Base<V, T1, T2> : GameEventBase where V : GameEvent2Base<V, T1, T2> {
        public abstract void Raise(T1 arg1, T2 arg2);

        public virtual void AddListener(GameEventListener<V, T1, T2> listener) {
            #if UNITY_EDITOR
            base.AddListener(listener);
            #endif
        }

        public virtual void RemoveListener(GameEventListener<V, T1, T2> listener) {
            #if UNITY_EDITOR
            base.RemoveListener(listener);
            #endif
        }
    }
}
