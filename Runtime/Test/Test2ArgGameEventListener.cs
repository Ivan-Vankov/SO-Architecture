using ExtEvents;

namespace Vaflov {
    [UnityEngine.AddComponentMenu("")]
    public class Test2ArgGameEventListener : GameEventListener2Base<Test2ArgGameEvent, int, CustomData> {
        [EventArguments("testArg1", "testArg2")]
        public ExtEvent<int, CustomData> response;
        public override ExtEvent<int, CustomData> Response => response;
    }
}
