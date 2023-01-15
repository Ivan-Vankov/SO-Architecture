using ExtEvents;

namespace Vaflov {
    public class Test2ArgGameEventListener : GameEventListener<Test2ArgGameEvent, int, CustomData> {
        [EventArguments("testArg1", "testArg2")]
        public ExtEvent<int, CustomData> response;
        public override ExtEvent<int, CustomData> Response => response;
    }
}
