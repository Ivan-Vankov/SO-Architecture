using System;
using TypeReferences;
using UnityEditor;
using UnityEngine;

namespace Vaflov {
    public class GenericSOGenerator : ScriptableObject {
        public const int MAX_TYPE_COUNT = 3;
        [Range(0, MAX_TYPE_COUNT)]
        public int typeCount = 1;
        public TypeReference[] types;

        [HideInInspector] public SerializedObject serializedObject;
        [HideInInspector] public SerializedProperty typesProp;
        [HideInInspector] public SerializedProperty typeCountProp;

        public Action onGenericSOCreated;

        public class ClassChangeData {
            public Type type;
            public string error;
            public string fileContents;
            public string className;
        }

        //public string ChangeClassEditorInstanceIDKey => "ChangedListenerEditorInstanceID" + GetInstanceID();

        public string classChangeNamespace;
        public int forcedTypeCount = -1;
        public bool HasForcedTypeCount => forcedTypeCount != -1;
        public int TypeCount => HasForcedTypeCount ? forcedTypeCount : typeCount;

        public Type zeroGenericArgTargetType;
        public string baseClassName;
        public string classDirectory;

        public static GenericSOGenerator CreateGenericSOGenerator(string classChangeNamespace,
                                                                  Type zeroGenericArgTargetType,
                                                                  string baseClassName,
                                                                  string classDirectory,
                                                                  int forcedTypeCount = -1) {
            var genericSOGenerator = CreateInstance<GenericSOGenerator>();
            genericSOGenerator.classChangeNamespace = classChangeNamespace;
            genericSOGenerator.zeroGenericArgTargetType = zeroGenericArgTargetType;
            if (zeroGenericArgTargetType.IsGenericType) {
                Debug.LogError($"Can't use generic {zeroGenericArgTargetType} as a zero generic arg target type");
            }
            genericSOGenerator.baseClassName = baseClassName;
            genericSOGenerator.classDirectory = classDirectory;
            genericSOGenerator.forcedTypeCount = forcedTypeCount;
            return genericSOGenerator;
        }

        public void OnEnable() {
            types = new TypeReference[MAX_TYPE_COUNT];
            for (int i = 0; i < types.Length; i++) {
                types[i] = new TypeReference();
            }

            serializedObject = new SerializedObject(this);
            typesProp = serializedObject.FindProperty(nameof(types));
            typeCountProp = serializedObject.FindProperty(nameof(typeCount));
        }

        public void OnDisable() {
            serializedObject.Dispose();
        }

        public void DrawTypePicker() {
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
                    new GUIContent($"Type {i + 1}"));
            }
            //}

            string error = null;
            for (int i = 0; i < typeCount; i++) {
                if (types[i].Type == null) {
                    error = "Choose all the type parameters first!";
                    break;
                }
            }
            if (error != null) {
                GUI.enabled = false;
                GUILayout.Button(new GUIContent("Create Asset", error));
                GUI.enabled = true;
            } else if (GUILayout.Button("Create Asset")) {
                Debug.Log("here");
                //DestroyImmediate(this);
                //serializedObject.Dispose();
                onGenericSOCreated?.Invoke();
                return;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
