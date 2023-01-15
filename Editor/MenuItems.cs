using UnityEditor;
using UnityEngine;

namespace Vaflov {
    public class MenuItems : MonoBehaviour {
        [MenuItem("Assets/Create/" + Config.PACKAGE_NAME + "/Constant", priority = 10)]
        private static void CreateConstant() {
            var constantsEditor = ConstantsEditorWindow.Open();
            constantsEditor.OpenConstantCreationMenu();
        }

        [MenuItem("Assets/Create/" + Config.PACKAGE_NAME + "/Game Event", priority = 20)]
        private static void CreateEvent() {
            var gameEventsEditor = GameEventsEditorWindow.Open();
            gameEventsEditor.OpenGameEventCreationMenu();
        }
    }
}