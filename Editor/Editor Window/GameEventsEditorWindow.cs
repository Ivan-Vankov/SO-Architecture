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
using UnityEditor.Graphs;

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
                window.titleContent = new GUIContent("Game Events");
            }
            return window;
        }

        protected override void OnEnable() {
            createNewGameEvent = new CreateNewGameEvent();
            base.OnEnable();
        }

        protected override OdinMenuTree BuildMenuTree() {
            createNewGameEvent.ResetCachedTypes();
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

        public void OpenGameEventCreationMenu() {
            var selected = MenuTree?.Selection?.FirstOrDefault();
            if (selected == null || selected.Value is not CreateNewGameEvent) {
                createNewGameEvent.name = null;
            }
            TrySelectMenuItemWithObject(createNewGameEvent);
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

    public class CreateNewGameEvent {
        public class ArgData {
            public string argName;
            public Type argType;
            public VaflovTypeSelector typeSelector;
        }

        public const int MAX_ARG_COUNT = 3;

        [HideInInspector]
        public List<ArgData> argData = new List<ArgData>(MAX_ARG_COUNT) {
            new ArgData(),
            new ArgData(),
            new ArgData(),
        };

        [HideInInspector]
        public int argCount;

        [HideInInspector]
        public string name;

        [HideInInspector]
        public string nameError;

        [HideInInspector]
        public List<string> assetNames;

        [HideInInspector]
        public List<Type> types;

        public const int labelWidth = 70;

        public void ResetCachedTypes() {
            types = AssemblyUtilities.GetTypes(AssemblyTypeFlags.GameTypes).Where(x => {
                if (x.Name == null)
                    return false;
                if (x.IsGenericType)
                    return false;
                string text = x.Name.TrimStart(Array.Empty<char>());
                return text.Length != 0 && char.IsLetter(text[0]);
            }).ToList();
            foreach (var arg in argData) {
                arg.typeSelector = new VaflovTypeSelector(types, supportsMultiSelect: false) {
                    //FlattenTree = true,
                };
                arg.typeSelector.SelectionChanged += types => {
                    arg.argType = types.FirstOrDefault();
                };
            }

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

        //public string ValidateArgName(string targetName) {
        //    if (targetName.Contains(' ')) {
        //        return "Name contains whitespace";
        //    }
        //    return null;
        //}

        public string ValidateGameEventNameUniqueness(string targetName) {
            for (int i = 0; i < assetNames.Count; ++i) {
                if (string.Compare(assetNames[i], targetName, StringComparison.OrdinalIgnoreCase) == 0) {
                    return "Name is not unique";
                }
            }
            return null;
        }

        [OnInspectorGUI]
        private void OnInspectorGUI() {
            var error = false;
            void ErrorMessageBox(string errorMessage) {
                SirenixEditorGUI.ErrorMessageBox(errorMessage);
                error = true;
            }
            var currNameError = string.IsNullOrEmpty(name)
                ? "Name is empty"
                : nameError;
            if (!string.IsNullOrEmpty(currNameError)) {
                ErrorMessageBox(currNameError);
            }
            GUIHelper.PushLabelWidth(labelWidth);
            var oldName = name;
            //name = SirenixEditorFields.DelayedTextField(GUIHelper.TempContent("Name"), name);
            name = SirenixEditorFields.TextField(GUIHelper.TempContent("Name"), name);
            if (name != oldName) {
                nameError = ValidateGameEventNameUniqueness(name);
            }

            argCount = EditorGUILayout.IntSlider("Arg Count", argCount, 0, 3);
            
            GUIHelper.PopLabelWidth();

            for (int i = 0; i < argCount; ++i) {
                var arg = argData[i];
                using (new GUILayout.HorizontalScope()) {
                    using (new GUILayout.VerticalScope()) {
                        GUIHelper.PushLabelWidth(labelWidth);
                        // TODO: Proper arg name validation
                        if (string.IsNullOrEmpty(arg.argName)) {
                            ErrorMessageBox($"Arg {i} name is empty");
                        }
                        else if (arg.argName.Contains(' ')) {
                            ErrorMessageBox("Name contains a whitespace");
                        }
                        arg.argName = SirenixEditorFields.TextField(GUIHelper.TempContent($"Arg {i}"), arg.argName);
                        GUIHelper.PopLabelWidth();
                    }

                    using (new GUILayout.VerticalScope()) {
                        var targetType = arg.argType;
                        var targetTypeError = targetType == null ? "Type is empty" : null;
                        if (!string.IsNullOrEmpty(targetTypeError)) {
                            ErrorMessageBox(targetTypeError);
                        }
                        var typeText = targetType == null ? "Select Type" : targetType.GetNiceFullName();
                        var typeTextContent = new GUIContent(typeText);
                        var typeTextStyle = EditorStyles.layerMaskField;
                        var rect = GUILayoutUtility.GetRect(typeTextContent, typeTextStyle);
                        var typeLabelRect = rect.SetSize(labelWidth, rect.height);
                        var typeSelectorRect = new Rect(rect.x + labelWidth + 2, rect.y, Max(rect.width - labelWidth - 2, 0), rect.height);
                        EditorGUI.LabelField(typeLabelRect, GUIHelper.TempContent("Type"));

                        var typeSelector = arg.typeSelector;
                        OdinSelector<Type>.DrawSelectorDropdown(typeSelectorRect, typeTextContent, _ => {
                            typeSelector.SetSelection(targetType);
                            typeSelector.ShowInPopup(new Rect(-300f, 0f, 300f, 0f));
                            return typeSelector;
                        }, typeTextStyle);
                    }
                }
            }

            if (error) {
                using (new EditorGUI.DisabledScope(true)) {
                    GUILayout.Button(new GUIContent("Create Asset", "Fix all errors first"));
                }
            } else if (GUILayout.Button("Create Asset")) {
                Debug.Log("here");
                //ConstantsGenerator.GenerateConstantAsset(name, targetType);
            }
        }
    }
}
#endif