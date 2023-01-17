#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.Mathf;
using UnityEditor;
using UnityEngine;
using static Vaflov.ContextMenuItemShortcutHandler;
using static Vaflov.StringUtil;

namespace Vaflov {
    public class GameEventsEditorWindow : EditorObjectMenuEditorWindow {
        public static readonly Vector2Int DEFAULT_EDITOR_SIZE = new Vector2Int(600, 400);

        public override Type EditorObjBaseType => typeof(GameEventBase);

        public CreateNewGameEvent createNewGameEvent;

        [MenuItem("Tools/SO Architecture/Game Events Editor")]
        public static GameEventsEditorWindow Open() {
            return Open<GameEventsEditorWindow>("Game Events", DEFAULT_EDITOR_SIZE, "Game Events");
        }

        protected override void OnEnable() {
            createNewGameEvent = new CreateNewGameEvent();
            base.OnEnable();
            GameEventEditorEvents.OnGameEventEditorPropChanged += RebuildEditorGroups;
            GameEventEditorEvents.OnGameEventDuplicated += TrySelectMenuItemWithObject;
            //ConstantsGenerator.OnConstantAssetGenerated += TrySelectMenuItemWithObject;
        }

        protected override void OnDisable() {
            base.OnDisable();
            GameEventEditorEvents.OnGameEventEditorPropChanged -= RebuildEditorGroups;
            GameEventEditorEvents.OnGameEventDuplicated -= TrySelectMenuItemWithObject;
            //ConstantsGenerator.OnConstantAssetGenerated -= TrySelectMenuItemWithObject;
        }

        public void OpenGameEventCreationMenu() {
            var selected = MenuTree?.Selection?.FirstOrDefault();
            if (selected == null || selected.Value is not CreateNewGameEvent) {
                createNewGameEvent.name = CreateNewGameEvent.DEFAULT_GAME_EVENT_NAME;
            }
            TrySelectMenuItemWithObject(createNewGameEvent);
        }

        protected override OdinMenuTree BuildMenuTree() {
            var tree = base.BuildMenuTree();
            createNewGameEvent.Reset();
            var constantCreatorMenuItem = new EmptyOdinMenuItem(tree, "Add a new game event", createNewGameEvent);
            tree.AddMenuItemAtPath("", constantCreatorMenuItem);
            return tree;
        }

        public override List<OdinContextMenuItem> GetToolbarItems() {
            var items = new List<OdinContextMenuItem>();
            items.Add(new OdinContextMenuItem("Add a new game event", () => {
                OpenGameEventCreationMenu();
                // EditorIconsOverview.OpenEditorIconsOverview();
            }, KeyCode.N, EventModifiers.Control | EventModifiers.Shift, SdfIconType.PlusCircle));
            items.AddRange(base.GetToolbarItems());
            items.Add(new OdinContextMenuItem("Regenerate game events", () => {
                GameEventsGenerator.GenerateGameEvents();
                //ForceMenuTreeRebuild();
            }, KeyCode.S, EventModifiers.Control, SdfIconType.ArrowRepeat));
            return items;
        }
    }

    public class GameEventArgData {
        public string argName;
        public Type argType;
        public VaflovTypeSelector typeSelector;
    }

    public class CreateNewGameEvent {
        public const int MAX_ARG_COUNT = 6;

        [HideInInspector]
        public readonly List<GameEventArgData> argData = new List<GameEventArgData>(MAX_ARG_COUNT) {
            new GameEventArgData(),
            new GameEventArgData(),
            new GameEventArgData(),
            new GameEventArgData(),
            new GameEventArgData(),
            new GameEventArgData(),
        };

        [HideInInspector]
        public int argCount;

        public const string DEFAULT_GAME_EVENT_NAME = "New Game Event";

        [HideInInspector]
        public string name = DEFAULT_GAME_EVENT_NAME;

        [HideInInspector]
        public string nameError;

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

        public string ValidateGameEventName(string targetName) {
            for (int i = 0; i < assetNames.Count; ++i) {
                if (string.Compare(assetNames[i], targetName, StringComparison.OrdinalIgnoreCase) == 0) {
                    return "Name is not unique";
                }
            }
            targetName = targetName.RemoveWhitespaces();
            return ValidateArgName(targetName);
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

        [OnInspectorGUI]
        private void OnInspectorGUI() {
            var error = false;
            void ErrorMessageBox(string errorMessage) {
                if (string.IsNullOrEmpty(errorMessage))
                    return;
                SirenixEditorGUI.ErrorMessageBox(errorMessage);
                error = true;
            }
            ErrorMessageBox(nameError);
            GUIHelper.PushLabelWidth(labelWidth);
            var oldName = name;
            //name = SirenixEditorFields.DelayedTextField(GUIHelper.TempContent("Name"), name);
            name = SirenixEditorFields.TextField(GUIHelper.TempContent("Name"), name);
            if (name != oldName) {
                nameError = ValidateGameEventName(name);
            }
            GUIHelper.PopLabelWidth();
            GUIHelper.PushLabelWidth(70);
            argCount = EditorGUILayout.IntSlider("Arg Count", argCount, 0, MAX_ARG_COUNT);
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
                    GUILayout.Button(new GUIContent("Create Asset", "Fix all errors first"));
                }
            } else if (GUILayout.Button("Create Asset")) {
                var passedArgData = new List<GameEventArgData>(argCount);
                for (int i = 0; i < argCount; ++i) {
                    passedArgData.Add(argData[i]);
                }
                GameEventsGenerator.GenerateGameEventAsset(name, passedArgData);
            }
        }
    }
}
#endif