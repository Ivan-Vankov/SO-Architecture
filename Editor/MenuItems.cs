using UnityEditor;
using UnityEngine;

namespace Vaflov {
    public class MenuItems : MonoBehaviour {
        // TODO: Open the constants editor and select the "new constant" menu item
        [MenuItem("Assets/Create/" + Config.PACKAGE_NAME + "/Constant", priority = 0)]
        private static void CreateConstant() {
            var constantsEditor = ConstantsEditorWindow.Open();
            //constantsEditor.OnBeginGUI
            constantsEditor.OpenConstantCreationMenu();
        }
        //private static void CreateConstant() => TypeSelectionWindow.CreateSelectionWindow(typeof(Constant<>), types => {
        //    foreach (var type in types) {
        //        Debug.Log(type);
        //    }
        //});

        [MenuItem("Assets/Create/" + Config.PACKAGE_NAME + "/Game Event", priority = 0)]
        private static void CreateEvents() => MultiTypeSelectionWindow.CreateMultiTypeSelectionWindow();
    }
}