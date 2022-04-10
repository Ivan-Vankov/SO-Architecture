using UnityEditor;
using UnityEngine;

namespace Vaflov {
    [CustomEditor(typeof(GameEvent))]
    public class GameEventEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            if (GUILayout.Button("Raise")) {
                var gameEvent = target as GameEvent;
                gameEvent.Raise();
            }
        }
    }
}