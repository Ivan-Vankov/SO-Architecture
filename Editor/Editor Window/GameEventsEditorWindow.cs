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
using static Vaflov.EditorStringUtil;

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
                createNewGameEvent.ResetName();
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

    public class GameEventCreationData {
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
        public List<Type> types;

        public void Reset() {
            types = EditorTypeUtil.GatherPublicTypes();
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
    }

    public class CreateNewGameEvent {
        public GameEventCreationData creationData = new GameEventCreationData();

        [HideInInspector]
        public List<string> assetNames;

        public const int labelWidth = 40;

        public void ResetName() {
            creationData.name = GameEventCreationData.DEFAULT_GAME_EVENT_NAME;
        }

        public void Reset() {
            creationData.Reset();
            assetNames = AssetUtil.GetAssetPathsForType(typeof(GameEventBase));
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
            ErrorMessageBox(creationData.nameError);
            GUIHelper.PushLabelWidth(labelWidth);
            var oldName = creationData.name;
            //name = SirenixEditorFields.DelayedTextField(GUIHelper.TempContent("Name"), name);
            creationData.name = SirenixEditorFields.TextField(GUIHelper.TempContent("Name"), creationData.name);
            if (creationData.name != oldName) {
                creationData.nameError = EditorStringUtil.ValidateAssetName(creationData.name, assetNames);
            }
            GUIHelper.PopLabelWidth();
            GUIHelper.PushLabelWidth(70);
            creationData.argCount = EditorGUILayout.IntSlider("Arg Count", creationData.argCount, 0, GameEventCreationData.MAX_ARG_COUNT);
            GUIHelper.PopLabelWidth();
            for (int i = 0; i < creationData.argCount; ++i) {
                var arg = creationData.argData[i];
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
                var passedArgData = new List<GameEventArgData>(creationData.argCount);
                for (int i = 0; i < creationData.argCount; ++i) {
                    passedArgData.Add(creationData.argData[i]);
                }
                GameEventsGenerator.GenerateGameEventAsset(creationData.name, passedArgData);
            }
        }
    }
}
#endif