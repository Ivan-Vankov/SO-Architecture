#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Vaflov {
    public class GameEventListenerSOEditorWindow : EditorObjectMenuEditorWindow {
        public override Type EditorObjBaseType => typeof(GameEventListenerSO);

        [MenuItem("Tools/" + Config.PACKAGE_NAME + "/Game Event Listeners Editor", priority = 25)]
        public static GameEventListenerSOEditorWindow Open() => Open<GameEventListenerSOEditorWindow>("Listeners", "Listener Small");

        [MenuItem("Assets/Create/" + Config.PACKAGE_NAME + "/Game Event Listener", priority = 25)]
        public static void CreateGameEventListenerSOMenuItem() => Open().CreateGameEventListenerSO();

        public void CreateGameEventListenerSO() {
            var listenerAsset = CreateInstance<GameEventListenerSO>();
            var path = $"Assets/Resources/Listeners/Game Event Listener.asset";
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CreateAsset(listenerAsset, path);
            AssetDatabase.SaveAssets();
            ForceMenuTreeRebuild();
            TrySelectMenuItemWithObject(listenerAsset);
            EditorObject.FocusEditorObjName();
        }

        public override List<OdinContextMenuItem> GetToolbarItems() {
            var items = new List<OdinContextMenuItem>();
            items.Add(new OdinContextMenuItem("Add a new game event listener", CreateGameEventListenerSO,
                KeyCode.N, EventModifiers.Control | EventModifiers.Shift, SdfIconType.PlusCircle));
            items.AddRange(base.GetToolbarItems());
            return items;
        }
    }
}
#endif