using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Vaflov {
    public class GameEventListenerSOEditorWindow : EditorObjectMenuEditorWindow {
        public override Type EditorObjBaseType => typeof(GameEventListenerSO);

        [MenuItem("Tools/SO Architecture/Game Event Listeners Editor")]
        public static GameEventListenerSOEditorWindow Open() {
            return Open<GameEventListenerSOEditorWindow>("Listeners", "Listener Small");
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
                EditorObject.FocusEditorObjName();
            }, KeyCode.N, EventModifiers.Control | EventModifiers.Shift, SdfIconType.PlusCircle));
            items.AddRange(base.GetToolbarItems());
            return items;
        }
    }
}