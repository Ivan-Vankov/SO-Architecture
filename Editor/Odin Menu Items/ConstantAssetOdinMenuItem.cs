using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Reflection;
using UnityEngine;
using static Vaflov.TypeUtil;

namespace Vaflov {
    public class ConstantAssetOdinMenuItem : OdinMenuItem {
        public FieldInfo valueField;

        public ConstantAssetOdinMenuItem(OdinMenuTree tree, string name, UnityEngine.Object value) : base(tree, name, value) {
            valueField = GetFieldRecursive(value.GetType(), "value", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }

        public ConstantAssetOdinMenuItem(OdinMenuTree tree, string name, UnityEngine.Object value, FieldInfo valueField) : base(tree, name, value) {
            this.valueField = valueField;
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
}
