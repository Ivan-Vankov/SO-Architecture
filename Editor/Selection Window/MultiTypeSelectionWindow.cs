using System.Linq;
using TypeReferences;
using UnityEditor;
using UnityEngine;

namespace Vaflov {
    public class MultiTypeSelectionWindow : EditorWindow {
        public TypeReference[] types;
        [HideInInspector] public SerializedObject serializedObject;
        [HideInInspector] public SerializedProperty typesProp;
        [HideInInspector] public SerializedProperty typeCountProp;

        [Range(0, MAX_TYPE_COUNT)]
        public int typeCount = 1;

        public const int MAX_TYPE_COUNT = 3;

        [MenuItem("Tools/Test Multi Type Selection Window")]
        public static void CreateMultiTypeSelectionWindow() {
            var window = CreateInstance<MultiTypeSelectionWindow>();
            window.Show();
        }

        public void OnEnable() {
            titleContent = new GUIContent("Choose Arguments");

            types = new TypeReference[MAX_TYPE_COUNT];
            for (int i = 0; i < types.Length; i++) {
                types[i] = new TypeReference();
            }

            serializedObject = new SerializedObject(this);
            typesProp = serializedObject.FindProperty(nameof(types));
            typeCountProp = serializedObject.FindProperty(nameof(typeCount));

            EditorApplication.projectChanged += Close;
            EditorApplication.quitting += Close;
            AssemblyReloadEvents.beforeAssemblyReload += Close;
        }
        public void OnDestroy() {
            EditorApplication.projectChanged -= Close;
            EditorApplication.quitting -= Close;
            AssemblyReloadEvents.beforeAssemblyReload -= Close;
        }

        public void OnGUI() {
            serializedObject.Update();


            //// TODO: Do this only once. For some reason it resets when you choose a type
            //titleContent = new GUIContent("Choose Arguments");

            //typeCount = EditorGUILayout.IntSlider("Type Count", typeCount, 0, MAX_TYPE_COUNT);
            EditorGUILayout.PropertyField(typeCountProp);
            //if (typeCount == 1) {
            //    EditorGUILayout.PropertyField(typesProp.GetArrayElementAtIndex(0), new GUIContent($"Arg"));
            //} else {
                for (int i = 0; i < typeCount; i++) {
                    EditorGUILayout.PropertyField(
                        typesProp.GetArrayElementAtIndex(i),
                        new GUIContent($"Arg {i + 1}"));
                }
            //}

            if (GUILayout.Button("Create Asset")) {
                for (int i = 0; i < typeCount; i++) {
                    if (types[i].Type == null) {
                        Debug.LogWarning("Choose all the type parameters first!");
                        return;
                    }
                }
                Debug.Log("here");
                serializedObject.Dispose();
                Close();
                return; 
            }

            serializedObject.ApplyModifiedProperties();
            //if (!GUILayout.Button("Create Asset"))
            //    return;

            //if (_typeRefs.Any(typeRef => typeRef.Type == null)) {
            //    Debug.LogWarning("Choose all the type parameters first!");
            //} else {
            //    Close();
            //    var types = new Type[_typeRefs.Length];
            //    for (int i = 0; i < _typeRefs.Length; i++) {
            //        types[i] = _typeRefs[i].Type;
            //    }
            //    _onTypesSelected(types);
            //}
        }
    }
}