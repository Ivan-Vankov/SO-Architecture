using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static Vaflov.ContextMenuItemShortcutHandler;

namespace Vaflov {
    public class ContextMenuItemSelector : GenericSelector<OdinContextMenuItem> {
        [HideInInspector] public int offset;
        [HideInInspector] public int height;

        public ContextMenuItemSelector(IEnumerable<OdinContextMenuItem> collection, int offset = 5, int height = 20)
            : base(null, false, x => x.name, collection) {
            this.offset = offset;
            this.height = height;
            Init();
        }

        public void Init() {
            SelectionTree.EnumerateTree(x => {
                if (x.Value is not OdinContextMenuItem contextMenuItem
                || contextMenuItem.shortcut == KeyCode.None) {
                    return;
                }
                x.OnDrawItem += y => {
                    GUI.Label(y.Rect.Padding(offset, 0).AlignCenterY(height), contextMenuItem.shortcutFormated, SirenixGUIStyles.RightAlignedGreyMiniLabel);
                };
            });

            EnableSingleClickToSelect();
            SelectionTree.Config.DrawSearchToolbar = false;
            SelectionTree.DefaultMenuStyle.Offset = offset;
            SelectionTree.DefaultMenuStyle.Borders = false;
            SelectionTree.DefaultMenuStyle.Height = height;
            SelectionConfirmed += selection => selection.FirstOrDefault()?.action?.Invoke();
        }

        public OdinEditorWindow ShowInPopup(int width) {
            var window = base.ShowInPopup(width);
            List<OdinContextMenuItem> contextMenuItems = new List<OdinContextMenuItem>();
            SelectionTree.EnumerateTree(x => {
                if (x.Value is OdinContextMenuItem contextMenuItem && contextMenuItem.shortcut != KeyCode.None) {
                    contextMenuItems.Add(contextMenuItem);
                }
            }); 
            window.OnEndGUI += () => HandleContextMenuItemShortcuts(contextMenuItems, window.Close);
            return window;
        }
    }

    public class EditorObjectOdinMenuItem : OdinMenuItem {
        public EditorObjectOdinMenuItem(OdinMenuTree tree, string name, UnityEngine.Object value) : base(tree, name, value) {
            OnRightClick += OpenRightClickMenu;
        }

        public static void OpenRightClickMenu(OdinMenuItem self) {
            (self as EditorObjectOdinMenuItem).OpenRightClickMenu();
        }

        public void OpenRightClickMenu() {
            if (!MenuTree.Selection.Contains(this)) {
                Select(true);
            }
            var contextMenuItems = (Value as IEditorObject).GetContextMenuItems();
            new ContextMenuItemSelector(contextMenuItems).ShowInPopup(150);
        }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect) {
            //GUI.Label(rect, new GUIContent((string)null, SmartName + " test tooltip"));

            var editorObj = Value as IEditorObject;
            var valueLabel = editorObj.EditorToString();
            if (!string.IsNullOrEmpty(valueLabel)) {
                valueLabel = " " + valueLabel;
            }
            var labelStyle = IsSelected ? Style.SelectedLabelStyle : Style.DefaultLabelStyle;
            var nameLabelSize = labelStyle.CalcSize(GUIHelper.TempContent(SmartName));
            var valueRect = new Rect(labelRect.x + nameLabelSize.x, labelRect.y, labelRect.width - nameLabelSize.x, labelRect.height);
            var valueContent = new GUIContent(valueLabel);

            GUIHelper.PushColor(Color.cyan);
            GUI.Label(valueRect, valueContent, labelStyle);
            GUIHelper.PopColor();

            var commentLabel = editorObj.EditorComment;
            if (!string.IsNullOrEmpty(commentLabel)) {
                commentLabel = (" " + commentLabel).Replace('\n', ' ');
                var valueLabelSize = labelStyle.CalcSize(valueContent);
                var commentRect = new Rect(valueRect.x + valueLabelSize.x, valueRect.y, valueRect.width - valueLabelSize.x, valueRect.height);

                GUIHelper.PushColor(Color.green);
                GUI.Label(commentRect, commentLabel, labelStyle);
                GUIHelper.PopColor();
            }
        }
    }
}
