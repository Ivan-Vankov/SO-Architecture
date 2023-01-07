namespace Vaflov {
    public abstract class GameEvent2Base<T, U, V> : GameEventBase where V: GameEvent2Base<T, U, V> {
        public abstract void Raise(T arg1, U arg2);

        public virtual void AddListener(GameEventListener2Base<T, U, V> listener) {
            #if UNITY_EDITOR
            listeners.Add(listener);
            #endif
        }

        public virtual void RemoveListener(GameEventListener2Base<T, U, V> listener) {
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
