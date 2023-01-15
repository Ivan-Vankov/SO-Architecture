using ExtEvents;
using UnityEngine;

namespace Vaflov {
    public abstract class GameEventListenerBase : ScriptableObject {
        [HideInInspector]
        public UnityEngine.Object parent;

        public abstract void OnInit();
        public abstract void OnDone();
        public abstract void AssignGameEvent(GameEventBase gameEvent);
    }

    public abstract class GameEventListener<V, T> : GameEventListenerBase where V : GameEvent1Base<V, T> {
        [HideInInspector]
        public V eventRef;
        public abstract ExtEvent<T> Response { get; }

        public void CallResponse(T arg1) {
            Response?.Invoke(arg1);
        }

        public override void OnInit() {
            if (eventRef) {
                eventRef.AddListener(this);
            }
        }

        public override void OnDone() {
            if (eventRef) {
                eventRef.RemoveListener(this);
            }
        }

        public override void AssignGameEvent(GameEventBase gameEvent) {
            eventRef = (V)gameEvent;
        }
    }

    public abstract class GameEventListener<V, T1, T2> : GameEventListenerBase where V : GameEvent2Base<V, T1, T2> {
        [HideInInspector]
        public V eventRef;
        public abstract ExtEvent<T1, T2> Response { get; }

        public void CallResponse(T1 arg1, T2 arg2) {
            Response?.Invoke(arg1, arg2);
        }

        public override void OnInit() {
            if (eventRef) {
                eventRef.AddListener(this);
            }
        }

        public override void OnDone() {
            if (eventRef) {
                eventRef.RemoveListener(this);
            }
        }

        public override void AssignGameEvent(GameEventBase gameEvent) {
            eventRef = (V)gameEvent;
        }
    }

    public abstract class GameEventListener<V, T1, T2, T3> : GameEventListenerBase where V : GameEvent3Base<V, T1, T2, T3> {
        [HideInInspector]
        public V eventRef;
        public abstract ExtEvent<T1, T2, T3> Response { get; }

        public void CallResponse(T1 arg1, T2 arg2, T3 arg3) {
            Response?.Invoke(arg1, arg2, arg3);
        }

        public override void OnInit() {
            if (eventRef) {
                eventRef.AddListener(this);
            }
        }

        public override void OnDone() {
            if (eventRef) {
                eventRef.RemoveListener(this);
            }
        }

        public override void AssignGameEvent(GameEventBase gameEvent) {
            eventRef = (V)gameEvent;
        }
    }

    public abstract class GameEventListener<V, T1, T2, T3, T4> : GameEventListenerBase where V : GameEvent4Base<V, T1, T2, T3, T4> {
        [HideInInspector]
        public V eventRef;
        public abstract ExtEvent<T1, T2, T3, T4> Response { get; }

        public void CallResponse(T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            Response?.Invoke(arg1, arg2, arg3, arg4);
        }

        public override void OnInit() {
            if (eventRef) {
                eventRef.AddListener(this);
            }
        }

        public override void OnDone() {
            if (eventRef) {
                eventRef.RemoveListener(this);
            }
        }

        public override void AssignGameEvent(GameEventBase gameEvent) {
            eventRef = (V)gameEvent;
        }
    }

    public abstract class GameEventListener<V, T1, T2, T3, T4, T5> : GameEventListenerBase where V : GameEvent5Base<V, T1, T2, T3, T4, T5> {
        [HideInInspector]
        public V eventRef;
        public abstract ExtEvent<T1, T2, T3, T4, T5> Response { get; }

        public void CallResponse(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            Response?.Invoke(arg1, arg2, arg3, arg4, arg5);
        }

        public override void OnInit() {
            if (eventRef) {
                eventRef.AddListener(this);
            }
        }

        public override void OnDone() {
            if (eventRef) {
                eventRef.RemoveListener(this);
            }
        }

        public override void AssignGameEvent(GameEventBase gameEvent) {
            eventRef = (V)gameEvent;
        }
    }

    public abstract class GameEventListener<V, T1, T2, T3, T4, T5, T6> : GameEventListenerBase where V : GameEvent6Base<V, T1, T2, T3, T4, T5, T6> {
        [HideInInspector]
        public V eventRef;
        public abstract ExtEvent<T1, T2, T3, T4, T5, T6> Response { get; }

        public void CallResponse(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
            Response?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public override void OnInit() {
            if (eventRef) {
                eventRef.AddListener(this);
            }
        }

        public override void OnDone() {
            if (eventRef) {
                eventRef.RemoveListener(this);
            }
        }

        public override void AssignGameEvent(GameEventBase gameEvent) {
            eventRef = (V)gameEvent;
        }
    }
}
