using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Vaflov {
    //[CreateAssetMenu(
    //    fileName = "Game Event", 
    //    menuName = "SO Architecture/Events/Game Event")]
    public class GameEventVoid : GameEventBase {
        public Action action;

        [Button]
        public void Raise() {
            action?.Invoke();
        }
    }
}