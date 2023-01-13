using ExtEvents;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using UnityEngine;

namespace Vaflov {
    [CreateAssetMenu(
        fileName = "Test SO Listener",
        menuName = "SO Architecture/Test SO Listener")]
    public class Test2ArgSOGameEventListener : ScriptableObject, IGameEventListener {
        [HideInInspector]
        public Test2ArgGameEvent gameEvent;

        [LabelWidth(80)]
        [ShowInInspector]
        public Test2ArgGameEvent GameEvent {
            get => gameEvent;
            set {
                RemoveListener();
                gameEvent = value;
                AddListener();
            }
        }

        [PropertyOrder(10)]
        [EventArguments("testArg1", "testArg2")]
        public ExtEvent<int, CustomData> response;

        public void CallResponse(int arg1, CustomData arg2) {
            response?.Invoke(arg1, arg2);
        }

        public void AddListener() {
            Debug.Log("AddListener");
            if (gameEvent != null) {
                gameEvent.AddListener(this);
                gameEvent.action += CallResponse;
            }
        }

        public void RemoveListener() {
            Debug.Log("RemoveListener");
            if (gameEvent != null) {
                gameEvent.RemoveListener(this);
                gameEvent.action -= CallResponse;
            }
        }

        private void OnEnable() {
            AddListener();
        }

        private void OnDisable() {
            RemoveListener();
        }

        //public GameEventBase GameEvent { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    }
}
