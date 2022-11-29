using UnityEditor;
using UnityEngine;

namespace Vaflov {
    public class MultiTypeSelectionWindow : EditorWindow {
        [HideInInspector] public SerializedObject serializedObject;

        public GenericSOGenerator genericSOGenerator;

        [MenuItem("Tools/Test Multi Type Selection Window")]
        public static void CreateMultiTypeSelectionWindow() {
            var window = CreateInstance<MultiTypeSelectionWindow>();
            window.Show();
        }

        public void OnEnable() {
            titleContent = new GUIContent("Choose Arguments");
            //serializedObject = new SerializedObject(this);

            genericSOGenerator = GenericSOGenerator.CreateGenericSOGenerator(
                "namespace",
                typeof(GameEvent),
                "base class name",
                "class dir",
                -1
            );
            genericSOGenerator.onGenericSOCreated += Close;
            EditorApplication.projectChanged += Close;
            EditorApplication.quitting += Close;
            AssemblyReloadEvents.beforeAssemblyReload += Close;
        }

        public void OnDisable() {
            genericSOGenerator.onGenericSOCreated -= Close;
            DestroyImmediate(genericSOGenerator);
            EditorApplication.projectChanged -= Close;
            EditorApplication.quitting -= Close;
            AssemblyReloadEvents.beforeAssemblyReload -= Close;
        }

        public void OnGUI() {
            //serializedObject.Update();
            genericSOGenerator.DrawTypePicker();
            //if (this) {
            //    serializedObject.ApplyModifiedProperties();
            //}
        }
    }
}