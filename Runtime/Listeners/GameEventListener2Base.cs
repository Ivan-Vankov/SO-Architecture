using ExtEvents;
using UnityEngine;

namespace Vaflov {
    public abstract class GameEventListener2Base<T, U> : GameEventListenerBase {
        [HideInInspector]
        public abstract GameEvent2Base<T, U> EventRef { get; set; }
        public abstract ExtEvent<T, U> Response { get; }

        public void CallResponse(T arg1, U arg2) {
            Response?.Invoke(arg1, arg2);
        }

        //public void OnEnable() {
        //    if (EventRef) {
        //        EventRef.action += CallResponse;
        //    }
        //}

        //public void OnDisable() {
        //    if (EventRef) {
        //        EventRef.action -= CallResponse;
        //    }
        //}

        //#if UNITY_EDITOR && ODIN_INSPECTOR
        //public override void AssignGameEvent() {
        //    EventRef = (GameEvent2Arg<T, U>)gameEvent;
        //}
        //#endif
    }
}
