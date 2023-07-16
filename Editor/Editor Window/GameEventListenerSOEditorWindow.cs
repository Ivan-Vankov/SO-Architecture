#if ODIN_INSPECTOR && UNITY_EDITOR
using System;
using UnityEditor;

namespace Vaflov {
    public class GameEventListenerSOEditorWindow : EditorObjectMenuEditorWindow {
        public override Type EditorObjBaseType => typeof(GameEventListenerSO);
        public override string DefaultEditorObjFolderPath() => "Assets/Resources/Listeners";

        public override IEditorObjectCreator CreateEditorObjectCreator() =>
            new DefaultEditorObjectCreator<GameEventListenerSO>("Add", GameEventListenerSO.RESOURCES_PATH, "Game Event Listener");

        [MenuItem("Tools/" + SOArchitectureConfig.PACKAGE_NAME + "/Game Event Listeners Editor", priority = 25)]
        public static GameEventListenerSOEditorWindow Open() => Open<GameEventListenerSOEditorWindow>("Listeners", "Listener Small");

        [MenuItem("Assets/Create/" + SOArchitectureConfig.PACKAGE_NAME + "/Game Event Listener", priority = 25)]
        public static void CreateGameEventListenerSOMenuItem() => Open().TryOpenEditorObjectCreationMenu();
    }
}
#endif