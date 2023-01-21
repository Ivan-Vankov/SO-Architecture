using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using System;
using static UnityEngine.Mathf;
using System.Reflection;

namespace Vaflov {
    public class GameEventEditMenuDrawer : OdinValueDrawer<GameEventEditMenu> {
        public GameEventCreationData creationData = new GameEventCreationData();

        public const int labelWidth = 40;

        public ParameterInfo[] methodParams;

        [HideInInspector]
        protected override void Initialize() {
            //foldoutExpandedProp = this.Property.Children[nameof(GameEventEditMenu.foldoutExpanded)];
            creationData.Reset();
            methodParams = Property.ParentType.GetMethod("Raise")?.GetParameters();
            creationData.argCount = methodParams.Length;
            for (int i = 0; i < methodParams.Length; ++i) {
                var methodParam = methodParams[i];
                creationData.argData[i].SetNameAndType(methodParam.Name, methodParam.ParameterType);
            }
        }

        protected override void DrawPropertyLayout(GUIContent label) {
            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.BeginBoxHeader();
            Property.State.Expanded = SirenixEditorGUI.Foldout(Property.State.Expanded, "Edit Arguments");
            SirenixEditorGUI.EndBoxHeader();
            if (SirenixEditorGUI.BeginFadeGroup(this, Property.State.Expanded)) {
                var error = false;
                void ErrorMessageBox(string errorMessage) {
                    if (string.IsNullOrEmpty(errorMessage))
                        return;
                    SirenixEditorGUI.ErrorMessageBox(errorMessage);
                    error = true;
                }
                GUIHelper.PushLabelWidth(70);
                creationData.argCount = EditorGUILayout.IntSlider("Arg Count", creationData.argCount, 0, GameEventCreationData.MAX_ARG_COUNT);
                GUIHelper.PopLabelWidth();
                var sameArgs = false;
                Action<int, string, Type> sameArgsCheck = null;
                if (creationData.argCount == methodParams.Length) {
                    sameArgs = true;
                    sameArgsCheck = (i, name, type) => {
                        if (!sameArgs)
                            return;
                        var existingParam = methodParams[i];
                        sameArgs = existingParam.Name == name && existingParam.ParameterType == type;
                    };
                }
                for (int i = 0; i < creationData.argCount; ++i) {
                    var arg = creationData.argData[i];
                    SirenixEditorGUI.BeginBox(null);
                    GUIHelper.PushLabelWidth(labelWidth);

                    ErrorMessageBox(EditorStringUtil.ValidateArgName(arg.argName));

                    arg.argName = SirenixEditorFields.TextField(GUIHelper.TempContent($"Arg {i}"), arg.argName);
                    var targetType = arg.typeDropdownFieldDrawer.TypeField();
                    var targetTypeError = targetType == null ? "Type is empty" : null;
                    if (!string.IsNullOrEmpty(targetTypeError)) {
                        ErrorMessageBox(targetTypeError);
                    }
                    sameArgsCheck?.Invoke(i, arg.argName, targetType);

                    GUIHelper.PopLabelWidth();
                    SirenixEditorGUI.EndBox();
                }

                ErrorMessageBox(sameArgs ? "The existing game event has the same arguments" : null);

                if (error) {
                    using (new EditorGUI.DisabledScope(true)) {
                        GUILayout.Button(new GUIContent("Edit Game Event Args", "Fix all errors first"));
                    }
                } else if (GUILayout.Button("Edit Game Event Args")) {
                    var passedArgData = new List<GameEventArgData>(creationData.argCount);
                    for (int i = 0; i < creationData.argCount; ++i) {
                        passedArgData.Add(new GameEventArgData(creationData.argData[i]));
                    }
                    var gameEvent = Property.Parent.ValueEntry.WeakSmartValue as GameEventBase;
                    var name = gameEvent.name;
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(gameEvent));
                    AssetDatabase.SaveAssets();
                    GameEventsGenerator.GenerateGameEventAsset(name, passedArgData);
                }
            }

            SirenixEditorGUI.EndFadeGroup();
            SirenixEditorGUI.EndBox();
        }
    }
}
