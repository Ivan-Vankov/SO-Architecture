#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using static UnityEngine.Mathf;
using System;
using Sirenix.OdinInspector;
using static Vaflov.ContextMenuItemShortcutHandler;

namespace Vaflov {
    public class ConstantsEditorWindow : OdinMenuEditorWindow {
        public static readonly Vector2Int DEFAULT_EDITOR_SIZE = new Vector2Int(600, 400);

        public CreateNewConstant newConstantCreator;

        [MenuItem("Tools/SO Architecture/Constants Editor")]
        public static ConstantsEditorWindow Open() {
            var wasOpen = HasOpenInstances<ConstantsEditorWindow>();
            var window = GetWindow<ConstantsEditorWindow>();
            if (!wasOpen) {
                window.position = GUIHelper.GetEditorWindowRect().AlignCenter(DEFAULT_EDITOR_SIZE.x, DEFAULT_EDITOR_SIZE.y);
                window.MenuWidth = DEFAULT_EDITOR_SIZE.x / 2;
                var tex = Resources.Load<Texture2D>("pi");
                window.titleContent = new GUIContent("Constants", tex);
            }
            return window;
        }

        protected override void OnEnable() {
            newConstantCreator = new CreateNewConstant();
            base.OnEnable();
            ConstantEditorEvents.OnConstantEditorPropChanged += RebuildEditorGroups;
            ConstantEditorEvents.OnConstantDuplicated += TrySelectMenuItemWithObject;
            ConstantsGenerator.OnConstantAssetGenerated += TrySelectMenuItemWithObject;
        }

        protected override void OnDisable() {
            base.OnDisable();
            ConstantEditorEvents.OnConstantEditorPropChanged -= RebuildEditorGroups;
            ConstantEditorEvents.OnConstantDuplicated -= TrySelectMenuItemWithObject;
            ConstantsGenerator.OnConstantAssetGenerated -= TrySelectMenuItemWithObject;
        }

        public void OpenConstantCreationMenu() {
            var selected = MenuTree?.Selection?.FirstOrDefault();
            if (selected == null || selected.Value is not CreateNewConstant) {
                newConstantCreator.name = CreateNewConstant.DEFAULT_CONSTANT_NAME;
            }
            TrySelectMenuItemWithObject(newConstantCreator);
        }

        public void RebuildEditorGroups() {
            var oldSelectedObj = MenuTree.Selection.FirstOrDefault()?.Value;
            ForceMenuTreeRebuild();
            TrySelectMenuItemWithObject(oldSelectedObj);
        }

        protected override OdinMenuTree BuildMenuTree() {
            newConstantCreator.ResetCachedTypes();
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

            var types = TypeCache.GetTypesDerivedFrom(typeof(Constant<>))
                .Where(type => !type.IsGenericType)
                .ToList();
            //var constantTypes = AppDomain.CurrentDomain.GetAssemblies()
            //    .SelectMany(assembly => assembly.GetTypes())
            //    .Where(type => type.IsClass && !type.IsGenericType && !type.IsAbstract && IsInheritedFrom(type, typeof(Constant<>)))
            //    .ToList();

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
                var groupMenuItem = tree.GetMenuItem(groupName);
                if (groupMenuItem != null) {
                    groupMenuItem.OnDrawItem += x => {
                        var itemCountLabel = $" {x.ChildMenuItems.Count}";
                        var labelStyle = x.IsSelected ? x.Style.SelectedLabelStyle : x.Style.DefaultLabelStyle;
                        var nameLabelSize = labelStyle.CalcSize(GUIHelper.TempContent(x.SmartName));
                        var valueRect = new Rect(x.LabelRect.x + nameLabelSize.x, x.LabelRect.y, x.LabelRect.width - nameLabelSize.x, x.LabelRect.height);
                        var valueContent = GUIHelper.TempContent(itemCountLabel);

                        GUIHelper.PushColor(Color.green);
                        GUI.Label(valueRect, valueContent, labelStyle);
                        GUIHelper.PopColor();
                    };
                }
            }

            tree.EnumerateTree().ForEach(menuItem => menuItem.Toggled = true);

            var constantCreatorMenuItem = new EmptyOdinMenuItem(tree, "Add a new constant", newConstantCreator);
            //tree.Selection.SelectionChanged += _ => {
            //    tree.Config.AutoFocusSearchBar = !constantCreatorMenuItem.IsSelected;
            //    Debug.Log(tree.Config.AutoFocusSearchBar);
            //};
            tree.AddMenuItemAtPath("", constantCreatorMenuItem);

            return tree;
        }

        public List<ContextMenuItem> GetToolbarItems() {
            var items = new List<ContextMenuItem>();
            items.Add(new ContextMenuItem("Add a new constant", () => {
                OpenConstantCreationMenu();
                // EditorIconsOverview.OpenEditorIconsOverview();
            }, KeyCode.N, EventModifiers.Control | EventModifiers.Shift, SdfIconType.PlusCircle));
            var selected = MenuTree?.Selection?.FirstOrDefault();
            if (selected != null && selected.Value is IEditorObject editorObject) {
                items.AddRange(editorObject.GetContextMenuItems());
            }
            items.Add(new ContextMenuItem("Regenerate constants", () => {
                ConstantsGenerator.GenerateConstants();
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

    public class CreateNewConstant {
        [HideInInspector]
        public Type targetType = typeof(int);

        public const string DEFAULT_CONSTANT_NAME = "New Constant";

        [HideInInspector]
        public string name = DEFAULT_CONSTANT_NAME;

        [HideInInspector]
        public string nameError;

        [HideInInspector]
        public List<string> constantNames;

        [HideInInspector]
        public List<Type> types;

        [HideInInspector]
        public VaflovTypeSelector typeSelector;

        public const int labelWidth = 40;

        public void ResetCachedTypes() {
            types = AssemblyUtilities.GetTypes(AssemblyTypeFlags.GameTypes).Where(x => {
                if (x.Name == null || x.IsGenericType || x.IsNotPublic)
                    return false;
                string text = x.Name.TrimStart(Array.Empty<char>());
                return text.Length != 0 && char.IsLetter(text[0]);
            }).ToList();
            typeSelector = new VaflovTypeSelector(types, supportsMultiSelect: false) {
                //FlattenTree = true,
            };
            typeSelector.SelectionChanged += types => {
                targetType = types.FirstOrDefault();
            };

            constantNames = new List<string>();
            var constantTypes = TypeCache.GetTypesDerivedFrom(typeof(Constant<>))
                .Where(type => !type.IsGenericType)
                .ToList();
            foreach (var constantType in constantTypes) {
                var constantAssetGuids = AssetDatabase.FindAssets($"t: {constantType}");
                foreach (var constantAssetGuid in constantAssetGuids) {
                    var assetPath = AssetDatabase.GUIDToAssetPath(constantAssetGuid);
                    var constantAsset = AssetDatabase.LoadAssetAtPath(assetPath, constantType);
                    constantNames.Add(constantAsset.name);
                }
            }
        }

        public string ValidateConstantNameUniqueness(string targetName) {
            for (int i = 0; i < constantNames.Count; ++i) {
                if (string.Compare(constantNames[i], targetName, StringComparison.OrdinalIgnoreCase) == 0) {
                    return "Name is not unique";
                }
            }
            return null;
        }

        private OdinSelector<Type> SelectType(Rect arg) {
            typeSelector.SetSelection(targetType);
            typeSelector.ShowInPopup(new Rect(-300f, 0f, 300f, 0f));
            return typeSelector;
        }

        [OnInspectorGUI]
        private void OnInspectorGUI() {
            var currNameError = string.IsNullOrEmpty(name)
                ? "Name is empty"
                : nameError;
            if (!string.IsNullOrEmpty(currNameError)) {
                SirenixEditorGUI.ErrorMessageBox(currNameError);
            }
            GUIHelper.PushLabelWidth(labelWidth);
            var oldName = name;
            //name = SirenixEditorFields.DelayedTextField(GUIHelper.TempContent("Name"), name);
            name = SirenixEditorFields.TextField(GUIHelper.TempContent("Name"), name);
            if (name != oldName) {
                nameError = ValidateConstantNameUniqueness(name);
            }
            GUIHelper.PopLabelWidth();

            var targetTypeError = targetType == null ? "Type is empty" : null;
            if (!string.IsNullOrEmpty(targetTypeError)) {
                SirenixEditorGUI.ErrorMessageBox(targetTypeError);
            }
            var typeText = targetType == null ? "Select Type" : targetType.GetNiceFullName();
            var typeTextContent = new GUIContent(typeText);
            var typeTextStyle = EditorStyles.layerMaskField;
            var rect = GUILayoutUtility.GetRect(typeTextContent, typeTextStyle);
            var typeLabelRect = rect.SetSize(labelWidth, rect.height);
            var typeSelectorRect = new Rect(rect.x + labelWidth + 2, rect.y, Max(rect.width - labelWidth - 2, 0), rect.height);
            EditorGUI.LabelField(typeLabelRect, GUIHelper.TempContent("Type"));
            OdinSelector<Type>.DrawSelectorDropdown(typeSelectorRect, typeTextContent, SelectType, typeTextStyle);

            if (!string.IsNullOrEmpty(currNameError) || !string.IsNullOrEmpty(targetTypeError)) {
                using (new EditorGUI.DisabledScope(true)) {
                    GUILayout.Button(new GUIContent("Create Asset", "Fix all errors first"));
                }
            } else if (GUILayout.Button("Create Asset")) {
                ConstantsGenerator.GenerateConstantAsset(name, targetType);
            }
        }
    }
}

#endif