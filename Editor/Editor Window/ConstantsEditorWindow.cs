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
using static UnityEngine.Networking.UnityWebRequest;

namespace Vaflov {
    public class ConstantsEditorWindow : OdinMenuEditorWindow {

        [MenuItem("Tools/SO Architecture/Constants Editor")]
        public static void Open() {
            var window = GetWindow<ConstantsEditorWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
        }

        static OdinMenuStyle menuStyle;

        protected override OdinMenuTree BuildMenuTree() {
            var tree = new OdinMenuTree(true);
            tree.DefaultMenuStyle.IconSize = 28.00f;
            tree.Config.DrawSearchToolbar = true;

            menuStyle ??= new OdinMenuStyle() {
                TrianglePadding = 1.50f,
                AlignTriangleLeft = true,
            };
            tree.Config.DefaultMenuStyle = menuStyle;
            tree.DefaultMenuStyle = menuStyle;
            //tree.EnumerateTree().Where(x => x as OdinMenuStyle)

            var constantTypes = TypeCache.GetTypesDerivedFrom(typeof(Constant<>))
                .Where(type => !type.IsGenericType)
                .ToList();
            //var constantTypes = AppDomain.CurrentDomain.GetAssemblies()
            //    .SelectMany(assembly => assembly.GetTypes())
            //    .Where(type => type.IsClass && !type.IsGenericType && !type.IsAbstract && IsInheritedFrom(type, typeof(Constant<>)))
            //    .ToList();

            //var result = new HashSet<OdinMenuItem>();
            var groups = new Dictionary<string, HashSet<OdinMenuItem>>();
            //var groupList = new List<string>();


            foreach (var constantType in constantTypes) {
                var constantAssetGuids = AssetDatabase.FindAssets($"t: {constantType}");
                var groupField = GetFieldRecursive(constantType, "editorGroup", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                foreach (var constantAssetGuid in constantAssetGuids) {
                    var assetPath = AssetDatabase.GUIDToAssetPath(constantAssetGuid);
                    var constantAsset = AssetDatabase.LoadAssetAtPath(assetPath, constantType);

                    //tree.AddAssetAtPath("Constants", assetPath, constantType);

                    var menuItem = new OdinMenuItem(tree, constantAsset.name, constantAsset);
                    //tree.AddMenuItemAtPath(result, "Constants", menuItem);

                    //var groupField = GetFieldRecursive(constantType, "editorGroup", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    //var groupName = groupField.GetValue(constantAsset);
                    var groupName = groupField.GetValue(constantAsset) as string;
                    groupName = groupName == null || groupName == "" ? "Default" : groupName;

                    if(!groups.TryGetValue(groupName, out HashSet<OdinMenuItem> groupResult)) {
                        groupResult = new HashSet<OdinMenuItem>();
                        groups[groupName] = groupResult;
                    }
                    groupResult.Add(menuItem);
                    tree.AddMenuItemAtPath(groupResult, groupName, menuItem);
                }
            }
            //tree.AddAllAssetsAtPath("Game Events", "Assets/Resources", typeof(GameEvent), true, true);

            tree.EnumerateTree().ForEach(ShowTooltip);
            tree.EnumerateTree().ForEach(ShowValue);


            //tree.AddObjectAtPath("Config", menuStyle);
            return tree;
        }

        public void ShowTooltip(OdinMenuItem menuItem) {
            menuItem.OnDrawItem += x => {
                GUI.Label(x.Rect, new GUIContent("", x.SmartName + " test tooltip"));
                //var labelRect = x.LabelRect;
                //var age = $" --{(x.Value as Character).Age} years old";
                //var labelStyle = x.Style.DefaultLabelStyle;
                //var labelSize = labelStyle.CalcSize(GUIHelper.TempContent(x.SmartName));
                //var ageRect = new Rect(labelRect.x + labelSize.x, labelRect.y, labelRect.width - labelSize.x, labelRect.height);

                //GUIHelper.PushColor(Color.green);
                //GUI.Label(ageRect, age, labelStyle);
                //GUIHelper.PopColor();
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
                var valueLabel = " " + value.ToString();
                var labelStyle = x.IsSelected ? x.Style.SelectedLabelStyle : x.Style.DefaultLabelStyle;
                var nameLabelSize = labelStyle.CalcSize(GUIHelper.TempContent(x.SmartName));
                var valueRect = new Rect(labelRect.x + nameLabelSize.x, labelRect.y, labelRect.width - nameLabelSize.x, labelRect.height);
                var valueContent = new GUIContent(valueLabel);

                GUIHelper.PushColor(Color.cyan);
                GUI.Label(valueRect, valueContent, labelStyle);
                GUIHelper.PopColor();

                var commentField = GetFieldRecursive(valueType, "comment", bindingFlags);
                var comment = commentField.GetValue(x.Value);
                var commentLabel = comment.ToString();
                if (commentLabel == "") { return; }
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

            // Draws a toolbar with the name of the currently selected menu item.
            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
            if (selected != null) {
                GUILayout.Label(selected.Name);
            }

            if (SirenixEditorGUI.ToolbarButton(new GUIContent("Test toolbar button"))) {
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