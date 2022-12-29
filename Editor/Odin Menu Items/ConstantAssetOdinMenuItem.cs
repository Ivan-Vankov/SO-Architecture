using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using static Vaflov.CancellationTokenUtils;

namespace Vaflov {
    public class ContextMenuItemSelector : GenericSelector<ContextMenuItem> {
        [HideInInspector] public int offset;
        [HideInInspector] public int height;

        public ContextMenuItemSelector(IEnumerable<ContextMenuItem> collection, int offset = 5, int height = 20)
            : base(null, false, x => x.name, collection) {
            this.offset = offset;
            this.height = height;
            Init();
        }

        public void Init() {
            SelectionTree.EnumerateTree(x => {
                if (x.Value is not ContextMenuItem contextMenuItem
                || contextMenuItem.shortcut == KeyCode.None) {
                    return;
                }
                var shortcutStrBuilder = new StringBuilder();
                if (contextMenuItem.modifiers != EventModifiers.None) {
                    if (contextMenuItem.modifiers.HasFlag(EventModifiers.Control)) {
                        shortcutStrBuilder.Append("Ctrl+");
                    }
                    if (contextMenuItem.modifiers.HasFlag(EventModifiers.Alt)) {
                        shortcutStrBuilder.Append("Alt+");
                    }
                    if (contextMenuItem.modifiers.HasFlag(EventModifiers.Shift)) {
                        shortcutStrBuilder.Append("Shift+");
                    }
                }
                shortcutStrBuilder.Append(contextMenuItem.shortcut);
                var shortcutStr = shortcutStrBuilder.ToString();
                x.OnDrawItem += y => {
                    GUI.Label(y.Rect.Padding(offset, 0).AlignCenterY(height), shortcutStr, SirenixGUIStyles.RightAlignedGreyMiniLabel);
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
            ContextMenuItemShortcutHandler.HandleSelector(this);
            window.OnClose += () => ContextMenuItemShortcutHandler.CancelSelectorHandling(this);
            return window;
        }
    }

    public static class ContextMenuItemShortcutHandler {
        public static ContextMenuItemSelector selector;
        public static CancellationTokenSource cts;

        public static void HandleSelector(ContextMenuItemSelector _selector) {
            selector = _selector;
            cts = new CancellationTokenSource();
            HandleSelectorTask(cts.Token);
        }

        public static void CancelSelectorHandling(ContextMenuItemSelector _selector) {
            if (selector == _selector) {
                cts?.Cancel();
                cts?.Dispose();
            }
        }

        public static async void HandleSelectorTask(CancellationToken token) {
            try {
                while (!token.IsCancellationRequested) {
                    await Task.Delay(100, token);
                    if (token.IsCancellationRequested || selector == null) return;
                    Debug.Log(Event.current?.keyCode);
                    selector.SelectionTree.EnumerateTree(x => {
                        if (x.Value is not ContextMenuItem contextMenuItem
                            || contextMenuItem.shortcut == KeyCode.None) {
                            return;
                        }
                        //Debug.Log(Event.current.keyCode);
                        //Event.current.modifiers = EventModifiers
                    });
                }
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }
    }

    public class ConstantAssetOdinMenuItem : OdinMenuItem {
        public ConstantAssetOdinMenuItem(OdinMenuTree tree, string name, UnityEngine.Object value) : base(tree, name, value) {
            OnRightClick += OpenRightClickMenu;
        }

        public static void OpenRightClickMenu(OdinMenuItem self) {
            if (!self.MenuTree.Selection.Contains(self)) {
                self.Select(true);
            }
            var source = new List<ContextMenuItem>() {
                new ContextMenuItem("Duplicate", () => Debug.Log("duplicate"), KeyCode.D, EventModifiers.Control),
                new ContextMenuItem("Delete", () => Debug.Log("delete"), KeyCode.Delete),
            };
            new ContextMenuItemSelector(source).ShowInPopup(150);
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
            if (!string.IsNullOrEmpty(commentLabel)) {
                commentLabel = (" " + commentLabel).Trim('\n');
                var valueLabelSize = labelStyle.CalcSize(valueContent);
                var commentRect = new Rect(valueRect.x + valueLabelSize.x, valueRect.y, valueRect.width - valueLabelSize.x, valueRect.height);

                GUIHelper.PushColor(Color.green);
                GUI.Label(commentRect, commentLabel, labelStyle);
                GUIHelper.PopColor();
            }
        }
    }
}
