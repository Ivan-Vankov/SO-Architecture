#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using static Vaflov.TypeUtil;
using static UnityEngine.Mathf;
using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Validation;

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
            ConstantsGenerator.OnConstantAssetGenerated += TrySelectMenuItemWithObject;
        }

        protected override void OnDisable() {
            base.OnDisable();
            ConstantEditorEvents.OnConstantEditorPropChanged -= RebuildEditorGroups;
            ConstantsGenerator.OnConstantAssetGenerated -= TrySelectMenuItemWithObject;
        }

        public void OpenConstantCreationMenu() {
            newConstantCreator.Reset();
            TrySelectMenuItemWithObject(newConstantCreator);
        }

        public void RebuildEditorGroups() {
            var oldSelectedObj = MenuTree.Selection.FirstOrDefault()?.Value;
            ForceMenuTreeRebuild();
            TrySelectMenuItemWithObject(oldSelectedObj);
        }

        protected override OdinMenuTree BuildMenuTree() {
            var tree = new OdinMenuTree(true);
            tree.Selection.SupportsMultiSelect = false;
            tree.Config.DrawSearchToolbar = true;
            //tree.Config.AutoFocusSearchBar = false;

            var menuStyle = new OdinMenuStyle() {
                Borders = false,
                Height = 18,
                IconSize = 15f,
                TrianglePadding = 1.50f,
                AlignTriangleLeft = true,
            };
            tree.Config.DefaultMenuStyle = menuStyle;
            tree.DefaultMenuStyle = menuStyle;

            var constantTypes = TypeCache.GetTypesDerivedFrom(typeof(Constant<>))
                .Where(type => !type.IsGenericType)
                .ToList();
            //var constantTypes = AppDomain.CurrentDomain.GetAssemblies()
            //    .SelectMany(assembly => assembly.GetTypes())
            //    .Where(type => type.IsClass && !type.IsGenericType && !type.IsAbstract && IsInheritedFrom(type, typeof(Constant<>)))
            //    .ToList();

            var groups = new SortedDictionary<string, HashSet<UnityEngine.Object>>();
            foreach (var constantType in constantTypes) {
                var constantAssetGuids = AssetDatabase.FindAssets($"t: {constantType}");
                foreach (var constantAssetGuid in constantAssetGuids) {
                    var assetPath = AssetDatabase.GUIDToAssetPath(constantAssetGuid);
                    var constantAsset = AssetDatabase.LoadAssetAtPath(assetPath, constantType);
                    var groupName = (constantAsset as IEditorObject).EditorGroup;
                    groupName = groupName == null || groupName == "" ? "Default" : groupName;

                    if (!groups.TryGetValue(groupName, out HashSet<UnityEngine.Object> groupResult)) {
                        groupResult = new HashSet<UnityEngine.Object>();
                        groups[groupName] = groupResult;
                    }
                    groupResult.Add(constantAsset);
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
                    var menuItem = new ConstantAssetOdinMenuItem(tree, constant.name, constant);
                    groupResult.Add(menuItem);
                    tree.AddMenuItemAtPath(groupResult, groupName, menuItem);
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

        protected override void OnBeginDrawEditors() {
            if (MenuTree == null) { return; }
            var selected = MenuTree.Selection.FirstOrDefault();
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

            if (SirenixEditorGUIUtil.ToolbarButton(EditorIcons.Plus, toolbarHeight, tooltip: "Add a new constant")) {
                OpenConstantCreationMenu();
                //OneTypeSelectionWindow.ShowInPopup(200);
                //EditorIconsOverview.OpenEditorIconsOverview();
            }

            if (SirenixEditorGUIUtil.ToolbarButton(EditorIcons.X, toolbarHeight, tooltip: "Delete the selected constant")) {
                var selectedConstant = MenuTree.Selection.SelectedValue as UnityEngine.Object;
                string path = AssetDatabase.GetAssetPath(selectedConstant);
                if (!string.IsNullOrEmpty(path)) {
                    Debug.Log("Deleting asset " + path);
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.SaveAssets();
                }
            }

            if (SirenixEditorGUIUtil.ToolbarButton(EditorIcons.Refresh, toolbarHeight, tooltip: "Regenerate constants")) {
                ConstantsGenerator.GenerateConstants();
                ForceMenuTreeRebuild();
            }

            SirenixEditorGUI.EndHorizontalToolbar();
        }
    }

    public class ConstantAssetOdinMenuItem : OdinMenuItem {
        public FieldInfo valueField;
        
        public ConstantAssetOdinMenuItem(OdinMenuTree tree, string name, UnityEngine.Object value) : base(tree, name, value) {
            valueField = GetFieldRecursive(value.GetType(), "value", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect) {
            //GUI.Label(rect, new GUIContent((string)null, SmartName + " test tooltip"));

            var value = valueField.GetValue(Value);
            if (value == null) { return; }
            var valueLabel = value.ToString();
            if (valueLabel.Length > 100) {
                valueLabel = "...";
            } else {
                valueLabel = valueLabel.Replace("\n", " ");
            }
            valueLabel = " " + valueLabel;
            var labelStyle = IsSelected ? Style.SelectedLabelStyle : Style.DefaultLabelStyle;
            var nameLabelSize = labelStyle.CalcSize(GUIHelper.TempContent(SmartName));
            var valueRect = new Rect(labelRect.x + nameLabelSize.x, labelRect.y, labelRect.width - nameLabelSize.x, labelRect.height);
            var valueContent = new GUIContent(valueLabel);

            GUIHelper.PushColor(Color.cyan);
            GUI.Label(valueRect, valueContent, labelStyle);
            GUIHelper.PopColor();

            var commentLabel = (Value as IEditorObject)?.Comment;
            if (string.IsNullOrEmpty(commentLabel)) { return; }
            commentLabel = (" " + commentLabel).Trim('\n');
            var valueLabelSize = labelStyle.CalcSize(valueContent);
            var commentRect = new Rect(valueRect.x + valueLabelSize.x, valueRect.y, valueRect.width - valueLabelSize.x, valueRect.height);

            GUIHelper.PushColor(Color.green);
            GUI.Label(commentRect, commentLabel, labelStyle);
            GUIHelper.PopColor();
        }
    }

    public class EmptyOdinMenuItem : OdinMenuItem {
        public EmptyOdinMenuItem(OdinMenuTree tree, string name, object value) : base(tree, name, value) {
            Style = new OdinMenuStyle() {
                Height = 0,
                Borders = false,
            };
        }

        public override void DrawMenuItem(int indentLevel) {}
    }

    public class CreateNewConstant {
        [HideInInspector]
        public Type targetType;

        [HideInInspector]
        public string name;

        [HideInInspector]
        public string nameError;

        [HideInInspector]
        public List<string> constantNames;

        [HideInInspector]
        public List<Type> types;

        [HideInInspector]
        public TypeSelector typeSelector;

        public const int labelWidth = 40;

        public CreateNewConstant() {
            types = AssemblyUtilities.GetTypes(AssemblyTypeFlags.GameTypes).Where(delegate (Type x) {
                if (x.Name == null)
                    return false;
                if (x.IsGenericType)
                    return false;
                string text = x.Name.TrimStart(Array.Empty<char>());
                return text.Length != 0 && char.IsLetter(text[0]);
            }).ToList();
            typeSelector = new TypeSelector(types, supportsMultiSelect: false);
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

        public void Reset() {
            targetType = null;
            name = null;
            nameError = null;
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