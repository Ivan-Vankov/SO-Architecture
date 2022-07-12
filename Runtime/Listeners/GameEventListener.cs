using ExtEvents;

namespace Vaflov {
    public class GameEventListener : BaseGameEventListener {

        public GameEvent eventRef;
        public ExtEvent response;

        public void CallResponse() {
            response?.Invoke();
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