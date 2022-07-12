using System;
using System.IO;
using System.Security;
using TypeReferences;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using static UnityEngine.Mathf;

namespace Vaflov {
    public class GameEventClassGenerator : EditorWindow {
        public interface ISuccessState {
            void DrawState();
        }

        [Serializable]
        public class ErrorState : ISuccessState {
            public string errorText;

            public ErrorState(string errorText) {
                this.errorText = errorText;
            }

            public void DrawState() {
                if (string.IsNullOrEmpty(errorText)) { return; }
                GUILayout.Label(errorText, new GUIStyle {
                    normal = {
                        textColor = Color.red
                    },
                    padding = new RectOffset(5, 5, 0, 0),
                    wordWrap = true,
                });
            }
        }

        [Serializable]
        public class SuccessState : ISuccessState {
            public string successText;
            public string createdFilePath;

            public SuccessState(string successText, string createdFilePath) {
                this.successText = successText;
                this.createdFilePath = createdFilePath;
            }

            public void DrawState() {
                if (string.IsNullOrEmpty(this.successText)) { return; }
                //var successText = new GUIContent(this.successText);
                var successStyle = new GUIStyle {
                    normal = {
                        textColor = Color.green
                    },
                    padding = new RectOffset(5, 5, 0, 0),
                    wordWrap = true,
                };
                //var successRect = GUILayoutUtility.GetRect(successText, successStyle);

                //GUIStyle textStyle = EditorStyles.label;
                //textStyle.wordWrap = true;
                //EditorGUI.LabelField(textRect, text, textStyle);

                //GUI.Label(successRect, successText, successStyle);

                //var clickHereText = new GUIContent("Click here to select it");
                //var clickHereRect = GUILayoutUtility.GetRect(successText, EditorStyles.linkLabel);
                //EditorGUI.LinkButton(clickHereRect, clickHereText);

                // TODO: Do the actual selection
                EditorGUILayout.LinkButton("Click here to select the created file");
                EditorGUILayout.LabelField(this.successText, successStyle);

                //Handles.color = EditorStyles.linkLabel.normal.textColor;
                //var padding = EditorStyles.linkLabel.padding;
                //Handles.DrawLine(new Vector3(successRect.xMin + padding.left,  successRect.yMax), 
                //                 new Vector3(successRect.xMax - padding.right, successRect.yMax));
                //Handles.color = Color.white;
                //EditorGUIUtility.AddCursorRect(successRect, MouseCursor.Link);
                //GUI.Button(successRect, successText, successStyle);

                //GUILayout.Label(success, new GUIStyle {
                //    stretchHeight = true,
                //    normal = {
                //        textColor = Color.green
                //    }
                //}, GUILayout.Height(200));
            }
        }

        public bool useArg2;
        public TypeReference type1;
        public TypeReference type2;
        public string path = "SO Architecture/Custom";

        [SerializeReference]
        public ISuccessState successState;

        public SerializedObject serializedObject;

        public SerializedProperty type1Prop;
        public SerializedProperty type2Prop;
        public SerializedProperty pathProp;

        public void SetErrorState(string errorText) {
            successState = new ErrorState(errorText);
        }

        public void SetSuccessState(string successText, string createdFilePath) {
            successState = new SuccessState(successText, createdFilePath);
        }

        [MenuItem("Tools/SO Architecture/Generate Game Event Class")]
        public static void ShowWindow() {
            GetWindow<GameEventClassGenerator>(false, "Game Event Class Generator", true);
        }

        //public void CreateGUI() {
        //    serializedObject ??= new SerializedObject(this);

        //    rootVisualElement.Add(new Label("Hello"));
        //    var type1Prop = new PropertyField(serializedObject.FindProperty(nameof(type1)));
        //    type1Prop.BindProperty(serializedObject);
        //    rootVisualElement.Add(type1Prop);
        //}

        private void OnEnable() {
            serializedObject ??= new SerializedObject(this);
            type1Prop = serializedObject.FindProperty(nameof(type1));
            type2Prop = serializedObject.FindProperty(nameof(type2));
            pathProp  = serializedObject.FindProperty(nameof(path));
        }

        void OnGUI() {
            serializedObject.Update();
            TryCreateGameEventClass();
            successState?.DrawState();
            serializedObject.ApplyModifiedProperties();
        }

        public void TryCreateGameEventClass() {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(type1)));

            using (new EditorGUILayout.HorizontalScope()) {
                var prefixOffset = 3;
                var prefixRect = GUILayoutUtility.GetRect(EditorGUIUtility.labelWidth + prefixOffset, 18f, GUIStyle.none, GUILayout.ExpandWidth(expand: false));
                prefixRect.x += prefixOffset;
                prefixRect.width -= prefixOffset;
                EditorGUI.LabelField(prefixRect, "Type 2");

                Rect rect = GUILayoutUtility.GetRect(new GUIContent("non empty"), GUIStyle.none);
                var left_offset = 2;
                var toggle_offset = 20;
                var leftRect = new Rect(rect.x + left_offset, rect.y, toggle_offset, rect.height);
                useArg2 = EditorGUI.Toggle(leftRect, useArg2);

                var rectRight = new Rect(rect.x + left_offset + toggle_offset, rect.y, rect.width - toggle_offset - 5, rect.height);
                using (new EditorGUI.DisabledScope(!useArg2)) {
                    EditorGUI.PropertyField(rectRight, type2Prop, GUIContent.none);
                }
            }

            EditorGUILayout.PropertyField(pathProp, new GUIContent("Class Path"));
            if (!GUILayout.Button("Create Game Event Class")) { return; }

            if (type1 == null || type1.Type == null) {
                SetErrorState("Specify type 1");
                return;
            }

            if (useArg2 && (type2 == null || type2.Type == null)) {
                SetErrorState("Specify type 2");
                return;
            }

            var directoryPath = Path.GetFullPath(Path.Combine(Application.dataPath, path));
            if (!Directory.Exists(directoryPath)) {
                try {
                    Directory.CreateDirectory(directoryPath);
                } catch (Exception e) {
                    if (e is ArgumentException || e is IOException) {
                        SetErrorState("Path contains invalid symbols");
                    }
                    else if (e is SecurityException) {
                        SetErrorState("You don't have the required permissions to create this file");
                    }
                    else if (e is PathTooLongException) {
                        SetErrorState("Path extends the system max path length");
                    }
                    else {
                        SetErrorState("Error while creating the directory");
                    }
                    return;
                }
            }

            var fileName = type1.Type.Name + (useArg2 ? type2.Type.Name : "") + "GameEvent.cs";
            var fullFilePath = Path.Combine(directoryPath, fileName);

            if (FileUtil.TryCreateFileAsset("", fullFilePath)) {
                SetSuccessState($"Created {fileName} at path {fullFilePath}", fullFilePath);
            } else {
                SetErrorState($"Couldn't create {fileName} at path {fullFilePath}");
            }
        }
    }
}

