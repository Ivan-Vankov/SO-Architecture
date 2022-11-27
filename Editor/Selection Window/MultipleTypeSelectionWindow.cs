using System;
using System.Linq;
using SolidUtilities;
using SolidUtilities.Editor;
using TypeReferences;
using UnityEditor;
using UnityEngine;
using Vaflov;

namespace Vaflov {
    /// <summary>
    /// A window that has as many TypeReference fields as needed for the asset creation. The user has to choose all
    /// the generic argument types for the asset to be created.
    /// </summary>
    public class MultipleTypeSelectionWindow : EditorWindow, ITypeSelectionWindow {
        [HideInInspector] public const float WindowWidth = 300f;

        public TypeReferenceWithBaseTypes[] _typeRefs;

        [HideInInspector] public Action<Type[]> _onTypesSelected;
        [HideInInspector] public SerializedObject _serializedObject;
        [HideInInspector] public string[] _genericArgNames;

        public void CreateTypeSelectionWindow(Action<Type[]> onTypesSelected, string[] genericArgNames, Type[][] genericParamConstraints) {
            InitializeMembers(onTypesSelected, genericArgNames, genericParamConstraints);
            SubscribeToCloseWindow();

            this.Resize(WindowWidth, GetWindowHeight(_typeRefs.Length));
            this.CenterOnMainWin();
            Show();
        }

        public void InitializeMembers(Action<Type[]> onTypesSelected, string[] genericArgNames, Type[][] genericParamConstraints) {
            _onTypesSelected = onTypesSelected;
            _genericArgNames = genericArgNames;
            _typeRefs = GetTypeRefs(genericParamConstraints);
            titleContent = new GUIContent("Choose Arguments");
            _serializedObject = new SerializedObject(this);
        }

        public void OnGUI() {
            SerializedProperty typesArray = _serializedObject.FindProperty(nameof(_typeRefs));

            for (int i = 0; i < typesArray.arraySize; i++) {
                EditorGUILayout.PropertyField(
                    typesArray.GetArrayElementAtIndex(i),
                    GUIContentHelper.Temp(_genericArgNames[i]));
            }

            if (!GUILayout.Button("Create Asset"))
                return;

            if (_typeRefs.Any(typeRef => typeRef.Type == null)) {
                Debug.LogWarning("Choose all the type parameters first!");
            } else {
                Close();
                var types = new Type[_typeRefs.Length];
                for (int i = 0; i < _typeRefs.Length; i++) {
                    types[i] = _typeRefs[i].Type;
                }
                _onTypesSelected(types);
            }
        }

        public void SubscribeToCloseWindow() {
            EditorApplication.projectChanged += Close;
            EditorApplication.quitting += Close;
            AssemblyReloadEvents.beforeAssemblyReload += Close;
        }

        public void OnDestroy() {
            EditorApplication.projectChanged -= Close;
            EditorApplication.quitting -= Close;
            AssemblyReloadEvents.beforeAssemblyReload -= Close;
        }

        public static float GetWindowHeight(int typeFieldsCount) {
            float oneTypeFieldHeight = EditorStyles.popup.CalcHeight(GUIContent.none, 0f);
            const float buttonHeight = 24f;
            const float spacing = 2f;
            float windowHeight = (oneTypeFieldHeight + spacing) * typeFieldsCount + buttonHeight;
            return windowHeight;
        }

        public static TypeReferenceWithBaseTypes[] GetTypeRefs(Type[][] genericParamConstraints) {
            int typesCount = genericParamConstraints.Length;
            var typeRefs = new TypeReferenceWithBaseTypes[typesCount];

            for (int i = 0; i < typesCount; i++) {
                typeRefs[i] = new TypeReferenceWithBaseTypes {
                    BaseTypeNames = genericParamConstraints[i]
                        .Select(type => type != null
                            ? $"{type.FullName}, {type.GetShortAssemblyName()}"
                            : string.Empty)
                        .ToArray()
                };
            }

            return typeRefs;
        }
    }
}