using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Vaflov {
    public class EditorObjectGroupContextMenuItem : OdinMenuItem {
        public EditorObjectMenuEditorWindow editorObjMenu;

        public EditorObjectGroupContextMenuItem(OdinMenuTree tree, string name, Object value, EditorObjectMenuEditorWindow editorObjMenu) : base(tree, name, value) {
            this.editorObjMenu = editorObjMenu;
            OnRightClick += OpenRightClickMenu;
        }

        public static void OpenRightClickMenu(OdinMenuItem self) {
            (self as EditorObjectGroupContextMenuItem).OpenRightClickMenu();
        }

        public void OpenRightClickMenu() {
            if (!MenuTree.Selection.Contains(this))
                Select(true);
            var contextMenuItems = editorObjMenu.GetRightClickContextMenuItems();
            new ContextMenuItemSelector(contextMenuItems).ShowInPopup(150);
        }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect) {
            var itemCountLabel = $" {ChildMenuItems.Count}";
            var labelStyle = IsSelected ? Style.SelectedLabelStyle : Style.DefaultLabelStyle;
            var nameLabelSize = labelStyle.CalcSize(GUIHelper.TempContent(SmartName));
            var valueRect = new Rect(labelRect.x + nameLabelSize.x, labelRect.y, labelRect.width - nameLabelSize.x, labelRect.height);
            var valueContent = GUIHelper.TempContent(itemCountLabel);

            GUIHelper.PushColor(Color.green);
            GUI.Label(valueRect, valueContent, labelStyle);
            GUIHelper.PopColor();

            var editorObjBaseType = editorObjMenu.EditorObjBaseType;
            var dragNDroppedEditorObj = DragAndDropUtilities.DropZone(rect, null, editorObjBaseType);
            if (dragNDroppedEditorObj == null)
                return;
            if (ChildMenuItems.Count > 0) {
                var firstMenuItem = ChildMenuItems[0];
                if (firstMenuItem.Value != null
                    && TypeUtil.IsInheritedFrom(firstMenuItem.Value.GetType(), editorObjBaseType)) {
                    var sortKey = (firstMenuItem.Value as ISortKeyObject).SortKey;
                    var sortKeyObj = (dragNDroppedEditorObj as ISortKeyObject);
                    if (sortKeyObj.SortKey >= sortKey) {
                        sortKeyObj.SortKey = sortKey - 1;
                    }
                }
            }
            var editorGroup = GetFullPath();
            editorGroup = editorGroup == "Default" ? "" : editorGroup;
            (dragNDroppedEditorObj as IEditorObject).EditorGroup = editorGroup;
            UnityEditorEventUtility.DelayAction(editorObjMenu.RebuildEditorGroups);
        }
    }
}
