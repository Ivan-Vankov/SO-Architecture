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
using System;
using Sirenix.OdinInspector;

namespace Vaflov {
    public class ConstantsEditorWindow : OdinMenuEditorWindow {

        public class CreateNewConstant {
            [HideInInspector]
            //[HideLabel]
            //[LabelWidth(40)]
            //[LabelText("Type")]
            public Type targetType;

            [LabelWidth(40)]
            public string name;

            // This tooltip doesn't work
            //[Tooltip("Select a type")]
            //[DisableIf(nameof(IsSelectedTypeInvalid))]
            //[Button]
            //public void CreateAsset() {
            //    Debug.Log("Here");
            //}

            //public bool IsSelectedTypeInvalid() {
            //    return selectedType.Type == null;
            //}

            private OdinSelector<Type> SelectType(Rect arg) {
                var targetTypeFlags = AssemblyTypeFlags.GameTypes;
                TypeSelector typeSelector = new TypeSelector(targetTypeFlags, supportsMultiSelect: false);
                typeSelector.SelectionChanged += types => {
                    Type type = types.FirstOrDefault();
                    if (type != null) {
                        targetType = type;
                        //base.titleContent = new GUIContent(targetType.GetNiceName());
                    }
                };
                typeSelector.SetSelection(targetType);
                typeSelector.ShowInPopup(new Rect(-300f, 0f, 300f, 0f));
                return typeSelector;
            }


            [OnInspectorGUI]
            private void OnInspectorGUI() {
                var typeText = targetType == null ? "Select Type" : targetType.GetNiceName();
                typeText = typeText.Length <= 25 ? typeText : typeText.Substring(0, 25) + "...";
                var typeTextContent = new GUIContent(typeText);
                //var typeTextStyle = EditorStyles.toolbarButton;
                var typeTextStyle = EditorStyles.toolbarDropDown;
                var rect = GUILayoutUtility.GetRect(typeTextContent, typeTextStyle);
                //rect = rect.SetWidth(Mathf.Max(rect.width, maxSize.x));
                //Rect rect = GUILayoutUtility.GetRect(0f, 21f, SirenixGUIStyles.ToolbarBackground);
                //GUILayoutUtility.GetRect(typeText, EditorS)
                //Rect rect2 = rect.AlignRight(80f);
                //Rect rect3 = rect.SetXMax(rect2.xMin);
                OdinSelector<Type>.DrawSelectorDropdown(rect, typeTextContent, SelectType, typeTextStyle);



                if (targetType == null) {
                    using (new EditorGUI.DisabledScope(true)) {
                        GUILayout.Button(new GUIContent("Create Asset", "test tooltip"));
                    }
                } else if (GUILayout.Button("Create Asset")) {
                    Debug.Log("here");

                    //EditorIconsOverview.OpenEditorIconsOverview();

                    //DestroyImmediate(this);
                    //serializedObject.Dispose();
                    //onGenericSOCreated?.Invoke();
                }
            }
        }

        public static readonly Vector2Int DEFAULT_EDITOR_SIZE = new Vector2Int(600, 400);

        public CreateNewConstant newConstantCreator;

        [MenuItem("Tools/SO Architecture/Constants Editor")]
        public static void Open() {
            var window = GetWindow<ConstantsEditorWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(DEFAULT_EDITOR_SIZE.x, DEFAULT_EDITOR_SIZE.y);
            window.MenuWidth = DEFAULT_EDITOR_SIZE.x / 2;
            var tex = Resources.Load<Texture2D>("pi");
            window.titleContent = new GUIContent("Constants", tex);
        }

        protected override void OnEnable() {
            newConstantCreator = new CreateNewConstant();
            base.OnEnable();
            ConstantEditorEvents.OnConstantEditorPropChanged += RebuildEditorGroups;
        }

        protected override void OnDisable() {
            base.OnDisable();
            ConstantEditorEvents.OnConstantEditorPropChanged -= RebuildEditorGroups;
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
                foreach (var constaint in groupList) {
                    var menuItem = new OdinMenuItem(tree, constaint.name, constaint);
                    groupResult.Add(menuItem);
                    tree.AddMenuItemAtPath(groupResult, groupName, menuItem);
                }
            }

            tree.EnumerateTree().ForEach(ShowTooltip);
            tree.EnumerateTree().ForEach(ShowValue);
            tree.EnumerateTree().ForEach(menuItem => menuItem.Toggled = true);

            tree.Add("", newConstantCreator);

            return tree;
        }

        public void ShowTooltip(OdinMenuItem menuItem) {
            menuItem.OnDrawItem += x => {
                GUI.Label(x.Rect, new GUIContent("", x.SmartName + " test tooltip"));
            };
        }

        public void ShowValue(OdinMenuItem menuItem) {
            menuItem.OnDrawItem += x => {
                var labelRect = x.LabelRect;
                if (x.Value == null) { return; }

                var valueType = x.Value.GetType();
                var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                var valueField = GetFieldRecursive(valueType, "value", bindingFlags);
                var value = valueField.GetValue(x.Value);
                if (value == null) { return; }
                var valueLabel = " " + value.ToString();
                var labelStyle = x.IsSelected ? x.Style.SelectedLabelStyle : x.Style.DefaultLabelStyle;
                var nameLabelSize = labelStyle.CalcSize(GUIHelper.TempContent(x.SmartName));
                var valueRect = new Rect(labelRect.x + nameLabelSize.x, labelRect.y, labelRect.width - nameLabelSize.x, labelRect.height);
                var valueContent = new GUIContent(valueLabel);

                GUIHelper.PushColor(Color.cyan);
                GUI.Label(valueRect, valueContent, labelStyle);
                GUIHelper.PopColor();

                var commentLabel = (x.Value as IEditorObject)?.Comment;
                if (commentLabel == null || commentLabel == "") { return; }
                commentLabel = (" " + commentLabel).Trim('\n');
                var valueLabelSize = labelStyle.CalcSize(valueContent);
                var commentRect = new Rect(valueRect.x + valueLabelSize.x, valueRect.y, valueRect.width - valueLabelSize.x, valueRect.height);

                GUIHelper.PushColor(Color.green);
                GUI.Label(commentRect, commentLabel, labelStyle);
                GUIHelper.PopColor();
            };
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
                newConstantCreator.name = null;
                newConstantCreator.targetType = null;
                TrySelectMenuItemWithObject(newConstantCreator);
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
                } else {
                    Debug.Log("No asset path for " + selectedConstant);
                }
                //OneTypeSelectionWindow.ShowInPopup(200);
                //EditorIconsOverview.OpenEditorIconsOverview();
            }

            //if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus)) {
            //    OneTypeSelectionWindow.ShowInPopup(200);
            //}
            SirenixEditorGUI.EndHorizontalToolbar();
        }
    }
}

#endif