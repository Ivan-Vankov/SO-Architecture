using System;
using UnityEditor;
using UnityEngine;

namespace Vaflov {
    public class GameEventListenerSOEditorWindow : EditorObjectMenuEditorWindow {
        public static readonly Vector2Int DEFAULT_EDITOR_SIZE = new Vector2Int(600, 400);

        public override Type EditorObjBaseType => typeof(GameEventListenerSO);

        public CreateNewConstant newConstantCreator;

        [MenuItem("Tools/SO Architecture/Game Event Listeners Editor")]
        public static GameEventListenerSOEditorWindow Open() {
            return Open<GameEventListenerSOEditorWindow>("Listeners", DEFAULT_EDITOR_SIZE, "Listener");
        }

        protected override void OnEnable() {
            newConstantCreator = new CreateNewConstant();
            base.OnEnable();
            GameEventListenerSOEditorEvents.OnGameEventListenerSOPropChanged += RebuildEditorGroups;
            GameEventListenerSOEditorEvents.OnGameEventListenerSODuplicated += TrySelectMenuItemWithObject;
        }

        protected override void OnDisable() {
            base.OnDisable();
            GameEventListenerSOEditorEvents.OnGameEventListenerSOPropChanged -= RebuildEditorGroups;
            GameEventListenerSOEditorEvents.OnGameEventListenerSODuplicated -= TrySelectMenuItemWithObject;
        }
    }
}