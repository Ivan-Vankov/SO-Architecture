using ExtEvents;

namespace Vaflov {
    public class GameEventListener3Arg<T, U, V> : BaseGameEventListener {

        public GameEvent3Arg<T, U, V> eventRef;
        public ExtEvent<T, U, V> response;

        public void CallResponse(T arg1, U arg2, V arg3) {
            response?.Invoke(arg1, arg2, arg3);
        }

        public void OnEnable() {
            if (eventRef) {
                eventRef.action += CallResponse;
            }
        }

        public void OnDisable() {
            if (eventRef) {
                eventRef.action -= CallResponse;
            }
        }
    }
}