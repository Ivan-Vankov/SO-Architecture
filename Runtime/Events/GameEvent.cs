using System;
using UnityEngine;

namespace Vaflov {
    [CreateAssetMenu(fileName = "Game Event", menuName = "SO Architecture/Events/Game Event")]
    public class GameEvent : ScriptableObject {
        public Action action;

        public void Raise() {
            action?.Invoke();
        }
    }
}