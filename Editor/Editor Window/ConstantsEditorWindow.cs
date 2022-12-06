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
using static Vaflov.CancellationTokenUtils;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Vaflov {
    public class ConstantsEditorWindow : OdinMenuEditorWindow {

        public CancellationTokenSource rebuildEditorGroupsCTS;

        public static readonly Vector2Int DEFAULT_EDITOR_SIZE = new Vector2Int(600, 400);

        [MenuItem("Tools/SO Architecture/Constants Editor")]
        public static void Open() {
            var window = GetWindow<ConstantsEditorWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(DEFAULT_EDITOR_SIZE.x, DEFAULT_EDITOR_SIZE.y);
            window.MenuWidth = DEFAULT_EDITOR_SIZE.x / 2;
        }

        protected override void OnEnable() {
            base.OnEnable();
            ConstantEditorEvents.OnConstantEditorPropChanged += RebuildEditorGroupsDelayed;
        }

        protected override void OnDisable() {
            base.OnDisable();
            ConstantEditorEvents.OnConstantEditorPropChanged -= RebuildEditorGroupsDelayed;
        }

        public void RebuildEditorGroupsDelayed() {
            ResetCancellationTokenSource(ref rebuildEditorGroupsCTS);
            RebuildEditorGroupsTask(rebuildEditorGroupsCTS.Token);
        }

        public async void RebuildEditorGroupsTask(CancellationToken token) {
            try {
                await Task.Delay(500, token);
            } catch (TaskCanceledException) {
                // task cancellation is expected
            }
            if (token.IsCancellationRequested || this == null) { return; }

            try {
                var oldSelectedItem = MenuTree.Selection.FirstOrDefault();
                var oldSelectedObj = oldSelectedItem?.Value as UnityEngine.Object;
                ForceMenuTreeRebuild();
                if (oldSelectedObj != null) {
                    OdinMenuItem newSelectedItem = null;
                    MenuTree.EnumerateTree(menuItem => {
                        if (newSelectedItem != null) { return; }
                        var newSelectedObj = menuItem.Value as UnityEngine.Object;
                        if (oldSelectedObj == newSelectedObj) {
                            newSelectedItem = menuItem;
                        }
                    });
                    if (newSelectedItem != null) {
                        MenuTree.Selection.Add(newSelectedItem);
                    }
                }
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
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

            // TODO: Add icons with tooltips instead of text
            if (SirenixEditorGUI.ToolbarButton(new GUIContent("New"))) {
                //ScriptableObjectCreator.ShowDialog<Item>("Assets/Plugins/Sirenix/Demos/Sample - RPG Editor/Items", obj => {
                //    obj.Name = obj.name;
                //    base.TrySelectMenuItemWithObject(obj); // Selects the newly created item in the editor
                //});
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }
    }
}

#endif