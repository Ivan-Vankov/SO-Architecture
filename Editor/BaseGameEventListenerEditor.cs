using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using TypeReferences;
using System;
using System.Linq;
using UnityEditor.Callbacks;
using Microsoft.CSharp;
using System.CodeDom;
using System.Text;
using static UnityEngine.Mathf;
using static Vaflov.TypeUtil;
using System.IO;

namespace Vaflov {
    [CustomEditor(typeof(BaseGameEventListener), true)]
    public class BaseGameEventListenerEditor : Editor {

        public class ListenerChangeTypeData {
            public Type listenerType;
            public string error;
            public string listenerFileContents;
            public string listenerClassName;
        }

        public int listenerTypeCount;
        public const int maxListenerTypeCount = 3;
        public TypeReference type1;
        public TypeReference type2;
        public TypeReference type3;

        public bool foldoutExpanded = false;

        public static string CHANGED_LISTENER_EDITOR_INSTANCE_ID = "ChangedListenerEditorInstanceID";

        public string EditorPersistanceKey => nameof(BaseGameEventListenerEditor) + target.GetInstanceID();

        private void OnEnable() {
            var editorPersistanceKey = EditorPersistanceKey;
            if (EditorPrefs.HasKey(editorPersistanceKey)) {
                var data = EditorPrefs.GetString(editorPersistanceKey);
                JsonUtility.FromJsonOverwrite(data, this);
            }
        }

        private void OnDisable() {
            var data = JsonUtility.ToJson(this);
            EditorPrefs.SetString(EditorPersistanceKey, data);
        }

        private void OnDestroy() {
            // If the target is being destroyed
            if (target == null && target is object) {
                EditorPrefs.DeleteKey(EditorPersistanceKey);
            }
        }

        public override VisualElement CreateInspectorGUI() {
            var root = new VisualElement();

            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            var changeListenerFoldout = new Foldout {
                text = "Change Listener Type",
                value = foldoutExpanded,
            };
            changeListenerFoldout.RegisterCallback<ClickEvent>(evt => {
                foldoutExpanded = changeListenerFoldout.value;
            });

            var changeListenerButtonTooltip = new VisualElement();
            var changeListenerButton = new Button { text = "Change Listener Type" };
            changeListenerButtonTooltip.Add(changeListenerButton);

            changeListenerButton.RegisterCallback<ClickEvent>(evt => TryChangeListenerType());

            void UpdateChangeListenersButton() {
                var error = GetListenerChangeTypeData().error;
                if (error != null) {
                    changeListenerButtonTooltip.tooltip = error;
                    changeListenerButton.SetEnabled(false);
                } else {
                    changeListenerButtonTooltip.tooltip = null;
                    changeListenerButton.SetEnabled(true);
                }
            }

            var listenerTypeCountSlider = new SliderIntWithValue("Type Count", 0, maxListenerTypeCount);
            var editorSO = new SerializedObject(this);
            var listenerTypeCountProp = editorSO.FindProperty(nameof(listenerTypeCount));
            listenerTypeCountSlider.BindProperty(listenerTypeCountProp);

            listenerTypeCountSlider.RegisterValueChangedCallback(changeEvent => EditorApplication.delayCall += UpdateChangeListenersButton);

            var listenerTypesContainer = new VisualElement();
            listenerTypeCountSlider.RegisterValueChangedCallback(changeEvent => {
                listenerTypesContainer.Clear();
                for (int i = 1; i <= Min(maxListenerTypeCount, changeEvent.newValue); ++i) {
                    var propField = new PropertyField();
                    var prop = editorSO.FindProperty($"type{i}");
                    propField.BindProperty(prop);
                    propField.RegisterValueChangeCallback(evt => EditorApplication.delayCall += UpdateChangeListenersButton);
                    listenerTypesContainer.Add(propField);
                }
            });

            changeListenerFoldout.Add(listenerTypeCountSlider);
            changeListenerFoldout.Add(listenerTypesContainer);
            changeListenerFoldout.Add(changeListenerButtonTooltip);

            root.Add(changeListenerFoldout);

            return root;
        }

        public ListenerChangeTypeData GetListenerChangeTypeData() {
            Type listenerTargetType = null;
            string listenerFileContents = null;
            string listenerClassName = null;

            if (listenerTypeCount == 0) {
                listenerTargetType = typeof(GameEventListener);
            } else {
                var listenerTypes = new Type[] { type1, type2, type3 };
                for (int i = 0; i < listenerTypeCount; ++i) {
                    var listenerType = listenerTypes[i];
                    if (listenerType == null) {
                        return new ListenerChangeTypeData() { error = $"Type {i + 1} not defined" };
                    }
                }
                listenerTargetType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsClass && !type.IsAbstract && IsInheritedFrom(type, typeof(BaseGameEventListener)))
                    .FirstOrDefault(type => {
                        var typeArguments = type.BaseType.GenericTypeArguments;
                        if (typeArguments.Length != listenerTypeCount) { return false; }
                        for (int i = 0; i < listenerTypeCount; ++i) {
                            if (typeArguments[i] != listenerTypes[i]) { return false; }
                        }
                        return true;
                    });
                if (listenerTargetType == null) {
                    var listenerClassNameBuilder = new StringBuilder();
                    using var codeProvider = new CSharpCodeProvider();
                    var listenerGenericArguments = new StringBuilder();
                    for (int i = 0; i < listenerTypeCount; ++i) {
                        var listenerType = listenerTypes[i];
                        listenerClassNameBuilder.Append(listenerType.Name);
                        var listenerTypeRealName = codeProvider.GetTypeOutput(new CodeTypeReference(listenerType));
                        listenerGenericArguments.Append(listenerTypeRealName);
                        if (i != listenerTypeCount - 1) {
                            listenerGenericArguments.Append(", ");
                        }
                    }
                    listenerClassNameBuilder.Append("GameEventListener");
                    listenerClassName = listenerClassNameBuilder.ToString();
                    listenerFileContents = "namespace Vaflov {\n"
                                        + $"\tpublic class {listenerClassName} : GameEventListener{listenerTypeCount}Arg<{listenerGenericArguments}> {{ }}\n"
                                        + "}\n";
                }
            }
            var error = listenerTargetType == target.GetType()
                ? "Target type is the same as the existing type"
                : null;
            return new ListenerChangeTypeData() {
                listenerType = listenerTargetType,
                error = error,
                listenerFileContents = listenerFileContents,
                listenerClassName = listenerClassName,
            };
        }

        public void TryChangeListenerType() {
            var listenerChangeTypeData = GetListenerChangeTypeData();
            if (listenerChangeTypeData.error != null) {
                Debug.LogError(listenerChangeTypeData.error);
                return;
            }
            if (listenerChangeTypeData.listenerType == null) {
                EditorPrefs.SetInt(CHANGED_LISTENER_EDITOR_INSTANCE_ID, GetInstanceID());
                CreateNewListenerFile(listenerChangeTypeData.listenerFileContents, listenerChangeTypeData.listenerClassName);
            } else {
                ChangeListenerType(listenerChangeTypeData.listenerType);
            }
        }

        public void CreateNewListenerFile(string fileContents, string className) {
            var listenerDirectory = Path.Combine(Application.dataPath, "SO Architecture", "Listeners");
            FileUtil.TryCreateDirectoryAsset(listenerDirectory);
            // TODO: Assure that this path is not taken
            var fullFilePath = Path.Combine(listenerDirectory, $"{className}.cs");

            if (!FileUtil.TryCreateFileAsset(fileContents, fullFilePath)) {
                Debug.LogError($"Couldn't create file {className}.cs at {fullFilePath}");
            }
        }

        public void ChangeListenerType(Type newListenerType) {
            var listenerTarget = target as BaseGameEventListener;
            var gameObject = listenerTarget.gameObject;
            gameObject.AddComponent(newListenerType);
            DestroyImmediate(listenerTarget);
        }

        [DidReloadScripts]
        private static void TryCreateAndChangeListenerTypeDelayed() {
            if (!EditorPrefs.HasKey(CHANGED_LISTENER_EDITOR_INSTANCE_ID)) {
                return;
            }
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) {
                EditorApplication.delayCall += TryCreateAndChangeListenerTypeDelayed;
                return;
            }
            EditorApplication.delayCall += TryChangeListenerTypeStatic;
        }

        public static void TryChangeListenerTypeStatic() {
            if (!EditorPrefs.HasKey(CHANGED_LISTENER_EDITOR_INSTANCE_ID)) {
                Debug.LogError("No instance id set for changed listener editor");
                return;
            }
            var changedListenerInstanceID = EditorPrefs.GetInt(CHANGED_LISTENER_EDITOR_INSTANCE_ID);
            EditorPrefs.DeleteKey(CHANGED_LISTENER_EDITOR_INSTANCE_ID);
            var target = EditorUtility.InstanceIDToObject(changedListenerInstanceID) as BaseGameEventListenerEditor;
            if (target == null) {
                Debug.LogError("Listener editor instance id does not resolve to a valid object");
                return;
            }
            target.TryChangeListenerType();
        }
    }
}
