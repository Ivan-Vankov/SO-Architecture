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

namespace Vaflov {
    public class GameEventsEditorWindow : OdinMenuEditorWindow {
        public static readonly Vector2Int DEFAULT_EDITOR_SIZE = new Vector2Int(600, 400);

        public CreateNewGameEvent createNewGameEvent;

        [MenuItem("Tools/SO Architecture/Game Events Editor")]
        public static GameEventsEditorWindow Open() {
            var wasOpen = HasOpenInstances<GameEventsEditorWindow>();
            var window = GetWindow<GameEventsEditorWindow>();
            if (!wasOpen) {
                window.position = GUIHelper.GetEditorWindowRect().AlignCenter(DEFAULT_EDITOR_SIZE.x, DEFAULT_EDITOR_SIZE.y);
                window.MenuWidth = DEFAULT_EDITOR_SIZE.x / 2;
                var tex = Resources.Load<Texture2D>("Game Events");
                window.titleContent = new GUIContent("Game Events", tex);
            }
            return window;
        }

        protected override void OnEnable() {
            createNewGameEvent = new CreateNewGameEvent();
            base.OnEnable();
        }

        public void OpenGameEventCreationMenu() {
            var selected = MenuTree?.Selection?.FirstOrDefault();
            if (selected == null || selected.Value is not CreateNewGameEvent) {
                createNewGameEvent.name = CreateNewGameEvent.DEFAULT_GAME_EVENT_NAME;
            }
            TrySelectMenuItemWithObject(createNewGameEvent);
        }

        protected override OdinMenuTree BuildMenuTree() {
            createNewGameEvent.Reset();
            var tree = new OdinMenuTree(true);
            tree.Selection.SupportsMultiSelect = false;
            tree.Config.DrawSearchToolbar = true;
            tree.Config.AutoFocusSearchBar = false;
            var menuStyle = new OdinMenuStyle() {
                Borders = false,
                Height = 18,
                IconSize = 15f,
                TrianglePadding = 1.50f,
                AlignTriangleLeft = true,
            };
            tree.Config.DefaultMenuStyle = menuStyle;
            tree.DefaultMenuStyle = menuStyle;

            var types = TypeCache.GetTypesDerivedFrom(typeof(GameEventBase))
                .Where(type => !type.IsGenericType)
                .ToList();

            var groups = new SortedDictionary<string, HashSet<UnityEngine.Object>>();
            foreach (var type in types) {
                var assetGuids = AssetDatabase.FindAssets($"t: {type}");
                foreach (var assetGuid in assetGuids) {
                    var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                    var asset = AssetDatabase.LoadAssetAtPath(assetPath, type);
                    var groupName = (asset as IEditorObject).EditorGroup;
                    groupName = groupName == null || groupName == "" ? "Default" : groupName;

                    if (!groups.TryGetValue(groupName, out HashSet<UnityEngine.Object> groupResult)) {
                        groupResult = new HashSet<UnityEngine.Object>();
                        groups[groupName] = groupResult;
                    }
                    groupResult.Add(asset);
                }
            }

            foreach ((var groupName, var group) in groups) {
                var groupList = group.ToList();
                groupList.Sort((UnityEngine.Object obj1, UnityEngine.Object obj2) => {
                    var sortKey1 = (obj1 as ISortKeyObject).SortKey;
                    var sortKey2 = (obj2 as ISortKeyObject).SortKey;
                    if (sortKey1 != sortKey2) {
                        return sortKey1.CompareTo(sortKey2);
                    } else {
                        return obj1.name.CompareTo(obj2.name);
                    }
                });
                var groupResult = new HashSet<OdinMenuItem>();
                foreach (var constant in groupList) {
                    var menuItem = new EditorObjectOdinMenuItem(tree, constant.name, constant);
                    menuItem.IconGetter = (constant as IEditorObject).GetEditorIcon;
                    groupResult.Add(menuItem);
                    tree.AddMenuItemAtPath(groupResult, groupName, menuItem);
                }
            }

            tree.EnumerateTree().ForEach(menuItem => menuItem.Toggled = true);

            var constantCreatorMenuItem = new EmptyOdinMenuItem(tree, "Add a new game event", createNewGameEvent);
            tree.AddMenuItemAtPath("", constantCreatorMenuItem);

            return tree;
        }

        public List<ContextMenuItem> GetToolbarItems() {
            var items = new List<ContextMenuItem>();
            items.Add(new ContextMenuItem("Add a new game event", () => {
                OpenGameEventCreationMenu();
                // EditorIconsOverview.OpenEditorIconsOverview();
            }, KeyCode.N, EventModifiers.Control | EventModifiers.Shift, SdfIconType.PlusCircle));
            var selected = MenuTree?.Selection?.FirstOrDefault();
            if (selected != null && selected.Value is IEditorObject editorObject) {
                items.AddRange(editorObject.GetContextMenuItems());
            }
            items.Add(new ContextMenuItem("Regenerate game events", () => {
                GameEventsGenerator.GenerateGameEvents();
                //ForceMenuTreeRebuild();
            }, KeyCode.S, EventModifiers.Control, SdfIconType.ArrowRepeat));
            return items;
        }

        protected override void OnBeginDrawEditors() {
            if (MenuTree == null)
                return;
            var toolbarHeight = MenuTree.Config.SearchToolbarHeight;
            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
            if (MenuTree.Selection != null) {
                var selectedNames = MenuTree.Selection?.Select(selected => selected.Name);
                var selectionLabel = string.Join(", ", selectedNames);
                if (selectionLabel?.Length > 40) {
                    selectionLabel = selectionLabel.Substring(0, 40) + "...";
                }
                GUILayout.Label(selectionLabel);
            }

            var toolbarItems = GetToolbarItems();
            foreach (var contextMenuItem in toolbarItems) {
                if (SirenixEditorGUIUtil.ToolbarSDFIconButton(contextMenuItem.icon, toolbarHeight, tooltip: contextMenuItem.tooltip)) {
                    contextMenuItem.action?.Invoke();
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();

            HandleContextMenuItemShortcuts(toolbarItems);
        }
    }

    public class GameEventArgData {
        public string argName;
        public Type argType;
        public VaflovTypeSelector typeSelector;
    }

    public class CreateNewGameEvent {
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
            targetName = new string(targetName.ToCharArray()
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());
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