using ExtEvents;

namespace Vaflov {
    public class GameEventListener1Arg<T> : BaseGameEventListener {

        public GameEvent1Arg<T> eventRef;
        public ExtEvent<T> response;

        public void CallResponse(T arg1) {
            response?.Invoke(arg1);
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