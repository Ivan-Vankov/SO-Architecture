using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Vaflov.ContextMenuItemShortcutHandler;
using System;

namespace Vaflov {
    public abstract class EditorObjectMenuEditorWindow : OdinMenuEditorWindow {
        public abstract Type EditorObjBaseType { get; } 

        public static T Open<T>(string title, Vector2Int size, string icon) where T : EditorObjectMenuEditorWindow {
            var wasOpen = HasOpenInstances<T>();
            var window = GetWindow<T>();
            if (!wasOpen) {
                window.position = GUIHelper.GetEditorWindowRect().AlignCenter(size.x, size.y);
                window.MenuWidth = size.x / 2;
                var tex = string.IsNullOrEmpty(icon) ? null : Resources.Load<Texture2D>(icon);
                window.titleContent = new GUIContent(title, tex);
            }
            return window;
        }

        public void RebuildEditorGroups() {
            var oldSelectedObj = MenuTree.Selection?.FirstOrDefault()?.Value;
            ForceMenuTreeRebuild();
            TrySelectMenuItemWithObject(oldSelectedObj);
        }

        protected override OdinMenuTree BuildMenuTree() {
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

            var types = TypeUtil.GetFlatTypesDerivedFrom(EditorObjBaseType);
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
                foreach (var editorObjAsset in groupList) {
                    var menuItem = new EditorObjectOdinMenuItem(tree, editorObjAsset.name, editorObjAsset);
                    menuItem.IconGetter = (editorObjAsset as IEditorObject).GetEditorIcon;
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
            return tree;
        }

        public virtual List<OdinContextMenuItem> GetToolbarItems() {
            var items = new List<OdinContextMenuItem>();
            var selected = MenuTree?.Selection?.FirstOrDefault();
            if (selected != null && selected.Value is IEditorObject editorObject) {
                items.AddRange(editorObject.GetContextMenuItems().Where(cmi => cmi.showInToolbar));
            }
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
}
