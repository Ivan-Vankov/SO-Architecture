using UnityEngine;
using UnityEditor;
using TypeReferences;
using System;
using Microsoft.CSharp;
using System.CodeDom;
using System.Text;
using System.IO;
using System.Collections.Generic;
using UnityEditor.Callbacks;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

namespace Vaflov {
    public abstract class ClassChangeEditor : Editor {
        public class ClassChangeData {
            public Type type;
            public string error;
            public string fileContents;
            public string className;
        }

        public bool foldoutExpanded = false;
        [Range(0, maxTypeCount)]
        public int typeCount;
        public const int maxTypeCount = 3;
        public TypeReference type1;
        public TypeReference type2;
        public TypeReference type3;

        public SerializedObject editorSO;
        public SerializedProperty foldoutExpandedProp;
        public SerializedProperty typeCountProp;
        public SerializedProperty type1Prop;
        public SerializedProperty type2Prop;
        public SerializedProperty type3Prop;

        public string ChangeClassEditorInstanceIDKey => "ChangedListenerEditorInstanceID" + GetInstanceID();

        public virtual string ClassChangeNamespace => typeof(ClassChangeEditor).Namespace;

        public virtual int ForcedTypeCount => -1;

        public bool HasForcedTypeCount => ForcedTypeCount != -1;
        public int TypeCount => HasForcedTypeCount ? ForcedTypeCount : typeCount;

        public abstract Type ZeroGenericArgTargetType { get; }
        public abstract string BaseClassName { get; }
        public abstract string ClassDirectory { get; }
        public abstract string FoldoutLabel { get; }

        public static Dictionary<string, EditorApplication.CallbackFunction> editorKeyToChangeClassAction = new Dictionary<string, EditorApplication.CallbackFunction>();

        public void OnEnable() {
            editorSO = new SerializedObject(this);
            foldoutExpandedProp = editorSO.FindProperty(nameof(foldoutExpanded));
            typeCountProp = editorSO.FindProperty(nameof(typeCount));
            type1Prop = editorSO.FindProperty(nameof(type1));
            type2Prop = editorSO.FindProperty(nameof(type2));
            type3Prop = editorSO.FindProperty(nameof(type3));

            editorKeyToChangeClassAction.Add(ChangeClassEditorInstanceIDKey, TryChangeTargetClassDelayed);
        }

        public void OnDisable() {
            editorKeyToChangeClassAction.Remove(ChangeClassEditorInstanceIDKey);
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            DrawChangeClassFoldoutSafe();
        }

        public void DrawChangeClassFoldoutSafe() {
            editorSO.Update();
            DrawChangeClassFoldout();
            if (this) { // Asset could have been destroyed
                editorSO.ApplyModifiedProperties();
            }
        }

        public void DrawChangeClassFoldout() {
            foldoutExpanded = EditorGUILayout.Foldout(foldoutExpanded, FoldoutLabel);
            if (foldoutExpanded) {
                if (!HasForcedTypeCount) {
                    typeCount = EditorGUILayout.IntSlider("Type Count", typeCount, 0, maxTypeCount);
                }
                if (TypeCount == 1) {
                    EditorGUILayout.PropertyField(type1Prop, new GUIContent("Type"));
                } else if (TypeCount > 0) {
                    var typePropArray = new SerializedProperty[] { type1Prop, type2Prop, type3Prop };
                    for (int i = 0; i < TypeCount; ++i) {
                        EditorGUILayout.PropertyField(typePropArray[i]);
                    }
                }
                var error = GetClassChangeData().error;
                if (error != null) {
                    GUI.enabled = false;
                    GUILayout.Button(new GUIContent(FoldoutLabel, error));
                    GUI.enabled = true;
                } else if (GUILayout.Button(FoldoutLabel)) {
                    TryChangeTargetClass();
                }
            }
        }

        public ClassChangeData GetClassChangeData() {
            Type targetType = null;
            string fileContents = null;
            string className = null;

            if (TypeCount == 0) {
                targetType = ZeroGenericArgTargetType;
            } else {
                var types = new Type[] { type1, type2, type3 };
                for (int i = 0; i < TypeCount; ++i) {
                    var type = types[i];
                    if (type == null) {
                        return new ClassChangeData() { error = $"Type {i + 1} not defined" };
                    }
                }

                var classNameBuilder = new StringBuilder();
                using var codeProvider = new CSharpCodeProvider();
                var genericArgumentsBuilder = new StringBuilder();
                for (int i = 0; i < TypeCount; ++i) {
                    var type = types[i];
                    var typeRealName = codeProvider.GetTypeOutput(new CodeTypeReference(type));
                    classNameBuilder.Append(char.ToUpper(typeRealName[0]) + typeRealName.Substring(1));
                    genericArgumentsBuilder.Append(typeRealName);
                    if (i != TypeCount - 1) {
                        genericArgumentsBuilder.Append(", ");
                    }
                }
                classNameBuilder.Append(BaseClassName);
                className = classNameBuilder.ToString();
                var baseClassName = ForcedTypeCount == 1
                    ? BaseClassName
                    : $"{BaseClassName}{TypeCount}Arg";
                fileContents = $"namespace {ClassChangeNamespace} {{\n"
                             + $"\tpublic class {className} : {baseClassName}<{genericArgumentsBuilder}> {{ }}\n"
                             + "}\n";

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; ++i) {
                    targetType = Type.GetType($"{ClassChangeNamespace}.{className}, {assemblies[i].FullName}");
                    if (targetType != null) { break; }
                }
            }
            var error = targetType == target.GetType()
                ? "Target type is the same as the existing type"
                : null;
            return new ClassChangeData() {
                type = targetType,
                error = error,
                fileContents = fileContents,
                className = className,
            };
        }

        public void TryChangeTargetClass() {
            var classChangeData = GetClassChangeData();
            if (classChangeData.error != null) {
                Debug.LogError(classChangeData.error);
                return;
            }
            if (classChangeData.type == null) {
                EditorPrefs.SetInt(ChangeClassEditorInstanceIDKey, GetInstanceID());
                CreateNewClassFile(classChangeData.fileContents, classChangeData.className);
            } else {
                ChangeTargetClass(classChangeData.type);
            }
        }

        public void CreateNewClassFile(string fileContents, string className) {
            var directoryPath = Path.Combine(Application.dataPath, "SO Architecture", ClassDirectory);
            FileUtil.TryCreateDirectoryAsset(directoryPath);
            // TODO: Assure that this path is not taken
            var fullFilePath = Path.Combine(directoryPath, $"{className}.cs");

            if (!FileUtil.TryCreateFileAsset(fileContents, fullFilePath)) {
                Debug.LogError($"Couldn't create file {className}.cs at {fullFilePath}");
            }
        }

        public void ChangeTargetClass(Type newTargetType) {
            if (target is Component targetComponent) {
                var gameObject = targetComponent.gameObject;
                gameObject.AddComponent(newTargetType);
                DestroyImmediate(targetComponent);
            } else if (target is ScriptableObject targetSO) {
                Debug.Log("here");
                var newSO = CreateInstance(newTargetType);
                var assetPath = AssetDatabase.GetAssetPath(targetSO);
                AssetDatabase.CreateAsset(newSO, assetPath);
                //EditorUtility.FocusProjectWindow();
                Selection.activeObject = newSO;
            } else {
                Debug.LogError("Unknown target type");
            }
        }


        [DidReloadScripts]
        private static void TryCreateAndChangeTargetClassesDelayed() {
            var editorChangeClassActions = new List<EditorApplication.CallbackFunction>();
            foreach (var entry in editorKeyToChangeClassAction) {
                if (EditorPrefs.HasKey(entry.Key)) {
                    editorChangeClassActions.Add(entry.Value);
                }
            }
            if (editorChangeClassActions.Count == 0) {
                return;
            }
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) {
                EditorApplication.delayCall += TryCreateAndChangeTargetClassesDelayed;
                return;
            }
            foreach (var editorChangeClassAction in editorChangeClassActions) {
                EditorApplication.delayCall += editorChangeClassAction;
            }
        }

        public void TryChangeTargetClassDelayed() {
            if (!EditorPrefs.HasKey(ChangeClassEditorInstanceIDKey)) {
                Debug.LogError("No instance id persisted for editor");
                return;
            }
            var editorInstanceID = EditorPrefs.GetInt(ChangeClassEditorInstanceIDKey);
            EditorPrefs.DeleteKey(ChangeClassEditorInstanceIDKey);
            var editorTarget = EditorUtility.InstanceIDToObject(editorInstanceID) as ClassChangeEditor;
            if (editorTarget == null) {
                Debug.LogError("Editor instance id does not resolve to a valid object");
                return;
            }
            editorTarget.TryChangeTargetClass();
        }
    }
}
