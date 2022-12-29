using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls.Expressions;
using UnityEditor;
using UnityEngine;

namespace Vaflov {
    public class ConstantAssetOdinMenuItem : OdinMenuItem {
        public ConstantAssetOdinMenuItem(OdinMenuTree tree, string name, UnityEngine.Object value) : base(tree, name, value) {
            //OnRightClick += OpenRightClickMenu;
        }

        public static void OpenRightClickMenu(OdinMenuItem self) {
            if (!self.MenuTree.Selection.Contains(self)) {
                self.Select(true);
            }
            var source = new List<ContextMenuItem>() {
                new ContextMenuItem("Duplicate", () => Debug.Log("duplicate"), KeyCode.Escape),
                new ContextMenuItem("Delete", () => Debug.Log("delete"), KeyCode.Delete),
            };
            var selector = new GenericSelector<ContextMenuItem>(null, false, x => x.name, source);

            //odinMenuItem2.OnDrawItem = (Action<OdinMenuItem>)Delegate.Combine(odinMenuItem2.OnDrawItem, (Action<OdinMenuItem>)delegate (OdinMenuItem x) {
            //    GUI.Label(x.Rect.Padding(10f, 0f).AlignCenterY(16f), t2.Namespace, SirenixGUIStyles.RightAlignedGreyMiniLabel);
            //});

            (var offset, var height) = (5, 20);

            selector.SelectionTree.EnumerateTree(x => {
                if (x.Value is not ContextMenuItem contextMenuItem || contextMenuItem.shortcut == null) { return; }
                x.OnDrawItem += y => {
                    GUI.Label(y.Rect.Padding(offset, 0).AlignCenterY(height), contextMenuItem.shortcut.ToString(), SirenixGUIStyles.RightAlignedGreyMiniLabel);
                };
            });

            selector.EnableSingleClickToSelect();
            selector.SelectionTree.Config.DrawSearchToolbar = false;
            selector.SelectionTree.DefaultMenuStyle.Offset = offset;
            selector.SelectionTree.DefaultMenuStyle.Borders = false;
            selector.SelectionTree.DefaultMenuStyle.Height = height;
            selector.SelectionConfirmed += selection => selection.FirstOrDefault()?.action?.Invoke();
            var window = selector.ShowInPopup(150);
        }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect) {
            //GUI.Label(rect, new GUIContent((string)null, SmartName + " test tooltip"));

            var editorObj = Value as IEditorObject;
            var valueLabel = " " + editorObj.EditorToString();
            var labelStyle = IsSelected ? Style.SelectedLabelStyle : Style.DefaultLabelStyle;
            var nameLabelSize = labelStyle.CalcSize(GUIHelper.TempContent(SmartName));
            var valueRect = new Rect(labelRect.x + nameLabelSize.x, labelRect.y, labelRect.width - nameLabelSize.x, labelRect.height);
            var valueContent = new GUIContent(valueLabel);

            GUIHelper.PushColor(Color.cyan);
            GUI.Label(valueRect, valueContent, labelStyle);
            GUIHelper.PopColor();

            var commentLabel = editorObj.EditorComment;
            if (string.IsNullOrEmpty(commentLabel)) { return; }
            commentLabel = (" " + commentLabel).Trim('\n');
            var valueLabelSize = labelStyle.CalcSize(valueContent);
            var commentRect = new Rect(valueRect.x + valueLabelSize.x, valueRect.y, valueRect.width - valueLabelSize.x, valueRect.height);

            GUIHelper.PushColor(Color.green);
            GUI.Label(commentRect, commentLabel, labelStyle);
            GUIHelper.PopColor();
        }
    }
}
