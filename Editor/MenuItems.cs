using UnityEditor;

namespace Vaflov {
    public class MenuItems {
        [MenuItem("Assets/Create/" + Config.PACKAGE_NAME + "/Constant", priority = 10)]
        private static void CreateConstant() {
            var constantsEditor = ConstantsEditorWindow.Open();
            constantsEditor.TryOpenEditorObjectCreationMenu();
        }

        [MenuItem("Assets/Create/" + Config.PACKAGE_NAME + "/Game Event", priority = 20)]
        private static void CreateEvent() {
            var gameEventsEditor = GameEventsEditorWindow.Open();
            gameEventsEditor.TryOpenEditorObjectCreationMenu();
        }

        [MenuItem("Assets/Create/" + Config.PACKAGE_NAME + "/Runtime Set", priority = 30)]
        private static void CreateRuntimeSet() {
            var gameEventsEditor = RuntimeSetEditorWindow.Open();
            gameEventsEditor.TryOpenEditorObjectCreationMenu();
        }
    }
}