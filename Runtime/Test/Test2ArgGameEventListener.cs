﻿using ExtEvents;
using UnityEngine;

namespace Vaflov {
    [AddComponentMenu("")]
    public class Test2ArgGameEventListener : GameEventListenerBase {
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
        [EventArguments("testArg1", "testArg2")]
        public ExtEvent<int, CustomData> response;

        public void CallResponse(int testArg1, CustomData testArg2) {
            response?.Invoke(testArg1, testArg2);
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

#if UNITY_EDITOR && ODIN_INSPECTOR
        public override void AssignGameEvent() {
            eventRef = (Test2ArgGameEvent)gameEvent;
        }
#endif
    }
}