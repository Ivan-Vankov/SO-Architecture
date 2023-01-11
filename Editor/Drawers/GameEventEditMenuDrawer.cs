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

namespace Vaflov {
    public class GameEventEditMenuDrawer : OdinValueDrawer<GameEventEditMenu> {
        //private InspectorProperty foldoutExpandedProp;

        protected override void Initialize() {
            //foldoutExpandedProp = this.Property.Children[nameof(GameEventEditMenu.foldoutExpanded)];
            Reset();
        }

        public const int MAX_ARG_COUNT = 3;

        [HideInInspector]
        public readonly List<GameEventArgData> argData = new List<GameEventArgData>(MAX_ARG_COUNT) {
            new GameEventArgData(),
            new GameEventArgData(),
            new GameEventArgData(),
        };

        [HideInInspector]
        public int argCount;

        public const string DEFAULT_GAME_EVENT_NAME = "New Game Event";

        [HideInInspector]
        public List<string> assetNames;

        [HideInInspector]
        public List<Type> types;

        public const int labelWidth = 40;

        public void Reset() {
            ResetCachedTypes();
            ResetArgData();
        }

        public void ResetCachedTypes() {
            types = AssemblyUtilities.GetTypes(AssemblyTypeFlags.GameTypes | AssemblyTypeFlags.PluginEditorTypes).Where(x => {
                if (x.Name == null)
                    return false;
                if (x.IsGenericType)
                    return false;
                string text = x.Name.TrimStart(Array.Empty<char>());
                return text.Length != 0 && char.IsLetter(text[0]);
            }).ToList();

            assetNames = new List<string>();
            var eventTypes = TypeCache.GetTypesDerivedFrom(typeof(GameEventBase))
                .Where(type => !type.IsGenericType)
                .ToList();
            foreach (var type in eventTypes) {
                var assetGuids = AssetDatabase.FindAssets($"t: {type}");
                foreach (var assetGuid in assetGuids) {
                    var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                    var asset = AssetDatabase.LoadAssetAtPath(assetPath, type);
                    assetNames.Add(asset.name);
                }
            }
        }

        public void ResetArgData() {
            for (int i = 0; i < argData.Count; i++) {
                var arg = argData[i];
                arg.argName = $"Arg{i}";
                arg.argType = typeof(int);
                arg.typeSelector = new VaflovTypeSelector(types, supportsMultiSelect: false) {
                    //FlattenTree = true,
                };
                arg.typeSelector.SelectionChanged += types => {
                    arg.argType = types.FirstOrDefault();
                };
            }
        }

        public string ValidateArgName(string argName) {
            if (argName.Length == 0)
                return "Name is empty";
            if (argName[0] != '_' && !char.IsLetter(argName[0]))
                return "The first character should be _ or a letter";
            for (int i = 1; i < argName.Length; ++i) {
                var c = argName[i];
                if (!char.IsLetter(c) && !char.IsDigit(c) && c != '_')
                    return "Name contains a character that is not \'_\', a letter or a digit";
            }
            return null;
        }

        protected override void DrawPropertyLayout(GUIContent label) {
            // TODO: Access the containing game event like this:
            //Debug.Log(Property.Parent.ValueEntry.WeakSmartValue.ToString());

            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.BeginBoxHeader();
            base.Property.State.Expanded = SirenixEditorGUI.Foldout(base.Property.State.Expanded, "Edit");
            SirenixEditorGUI.EndBoxHeader();
            if (SirenixEditorGUI.BeginFadeGroup(this, base.Property.State.Expanded)) {
                var error = false;
                void ErrorMessageBox(string errorMessage) {
                    if (string.IsNullOrEmpty(errorMessage))
                        return;
                    SirenixEditorGUI.ErrorMessageBox(errorMessage);
                    error = true;
                }
                GUIHelper.PushLabelWidth(70);
                argCount = EditorGUILayout.IntSlider("Arg Count", argCount, 0, 3);
                GUIHelper.PopLabelWidth();
                for (int i = 0; i < argCount; ++i) {
                    var arg = argData[i];
                    SirenixEditorGUI.BeginBox(null);
                    GUIHelper.PushLabelWidth(labelWidth);

                    ErrorMessageBox(ValidateArgName(arg.argName));

                    arg.argName = SirenixEditorFields.TextField(GUIHelper.TempContent($"Arg {i}"), arg.argName);
                    GUIHelper.PopLabelWidth();
                    var targetType = arg.argType;
                    var targetTypeError = targetType == null ? "Type is empty" : null;
                    if (!string.IsNullOrEmpty(targetTypeError)) {
                        ErrorMessageBox(targetTypeError);
                    }
                    var typeText = targetType == null ? "Select Type" : targetType.GetNiceFullName();
                    var typeTextContent = new GUIContent(typeText);
                    var typeTextStyle = EditorStyles.layerMaskField;
                    var rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, typeTextStyle);
                    var typeLabelRect = rect.SetSize(labelWidth, rect.height);
                    var typeSelectorRect = new Rect(rect.x + labelWidth + 2, rect.y, Max(rect.width - labelWidth - 2, 0), rect.height);
                    EditorGUI.LabelField(typeLabelRect, GUIHelper.TempContent("Type"));

                    var typeSelector = arg.typeSelector;
                    OdinSelector<Type>.DrawSelectorDropdown(typeSelectorRect, typeTextContent, _ => {
                        typeSelector.SetSelection(targetType);
                        typeSelector.ShowInPopup(new Rect(-300f, 0f, 300f, 0f));
                        return typeSelector;
                    }, typeTextStyle);
                    SirenixEditorGUI.EndBox();
                }

                if (error) {
                    using (new EditorGUI.DisabledScope(true)) {
                        GUILayout.Button(new GUIContent("Edit Game Event Args", "Fix all errors first"));
                    }
                } else if (GUILayout.Button("Edit Game Event Args")) {
                    var passedArgData = new List<GameEventArgData>(argCount);
                    for (int i = 0; i < argCount; ++i) {
                        passedArgData.Add(argData[i]);
                    }
                    //GameEventsGenerator.GenerateGameEventAsset(name, passedArgData);
                }
            }

            SirenixEditorGUI.EndFadeGroup();
            SirenixEditorGUI.EndBox();
        }
    }
}
