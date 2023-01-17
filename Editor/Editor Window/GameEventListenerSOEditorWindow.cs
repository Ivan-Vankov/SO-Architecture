using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
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

        public override List<OdinContextMenuItem> GetToolbarItems() {
            var items = new List<OdinContextMenuItem>();
            items.Add(new OdinContextMenuItem("Add a new game event listener", () => {
                var listenerAsset = CreateInstance<GameEventListenerSO>();
                var path = $"Assets/Resources/Listeners/Game Event Listener.asset";
                path = AssetDatabase.GenerateUniqueAssetPath(path);
                AssetDatabase.CreateAsset(listenerAsset, path);
                AssetDatabase.SaveAssets();
                ForceMenuTreeRebuild();
                TrySelectMenuItemWithObject(listenerAsset);
            }, KeyCode.N, EventModifiers.Control | EventModifiers.Shift, SdfIconType.PlusCircle));
            items.AddRange(base.GetToolbarItems());
            return items;
        }
    }
}