using ExtEvents;
using UnityEngine;

namespace Vaflov {
    [AddComponentMenu("")]
    public class Test2ArgGameEventListener : GameEventListener2Base<int, CustomData, Test2ArgGameEvent> {
        //[HideInInspector]
        //public Test2ArgGameEvent eventRef;
        //public override GameEvent2Base<int, CustomData> EventRef {
        //    get => eventRef;
        //    set => eventRef = (Test2ArgGameEvent)value;
        //}

        //[EventArguments("testArg1", "testArg2")]
        //public ExtEvent<int, CustomData> response;
        //public override ExtEvent<int, CustomData> Response => response;

        [HideInInspector]
        public Test2ArgGameEvent eventRef;
        [EventArguments(Test2ArgGameEvent.arg1Name, Test2ArgGameEvent.arg2Name)]
        public ExtEvent<int, CustomData> response;

        public override Test2ArgGameEvent EventRef { get => eventRef; set => eventRef = value; }
        public override ExtEvent<int, CustomData> Response => response;

        //public void CallResponse(int arg1, CustomData arg2) {
        //    response?.Invoke(arg1, arg2);
        //}

        //public void OnEnable() {
        //    if (eventRef) {
        //        eventRef.action += CallResponse;
        //    }
        //}

        //public void OnDisable() {
        //    if (eventRef) {
        //        eventRef.action -= CallResponse;
        //    }
        //}

        //#if UNITY_EDITOR
        //public override void AssignGameEvent() {
        //    eventRef = (Test2ArgGameEvent)gameEvent;
        //}
        //#endif
    }
}
