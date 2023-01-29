#if ODIN_INSPECTOR && UNITY_EDITOR
using System;
using UnityEditor;

namespace Vaflov {
    public class GameEventListenerSOEditorWindow : EditorObjectMenuEditorWindow {
        public override Type EditorObjBaseType => typeof(GameEventListenerSO);

        public override IEditorObjectCreator CreateEditorObjectCreator() =>
            new DefaultEditorObjectCreator<GameEventListenerSO>("Add a new game event listener", "Listeners", "Game Event Listener");

        [MenuItem("Tools/" + Config.PACKAGE_NAME + "/Game Event Listeners Editor", priority = 25)]
        public static GameEventListenerSOEditorWindow Open() => Open<GameEventListenerSOEditorWindow>("Listeners", "Listener Small");

        [MenuItem("Assets/Create/" + Config.PACKAGE_NAME + "/Game Event Listener", priority = 25)]
        public static void CreateGameEventListenerSOMenuItem() => Open().TryOpenEditorObjectCreationMenu();
    }
}
#endif