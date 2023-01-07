namespace Vaflov {
    public abstract class GameEvent1Base<V, T> : GameEventBase where V : GameEvent1Base<V, T> {
        public abstract void Raise(T arg1);

        public virtual void AddListener(GameEventListener1Base<V, T> listener) {
            #if UNITY_EDITOR
            listeners.Add(listener);
            #endif
        }

        public virtual void RemoveListener(GameEventListener1Base<V, T> listener) {
            #if UNITY_EDITOR
            var index = listeners.IndexOf(listener);
            if (index > -1) {
                listeners[index] = listeners[listeners.Count - 1];
                listeners.RemoveAt(listeners.Count - 1);
            }
            #endif
        }
    }
}
