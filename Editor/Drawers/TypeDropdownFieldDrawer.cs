using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Mathf;

namespace Vaflov {
    [Serializable]
    public class TypeDropdownFieldDrawer {
        public FormattedTypeSelector typeSelector;
        public Type targetType;

        public TypeDropdownFieldDrawer(List<Type> types) : this(types, typeof(int)) {}

        public TypeDropdownFieldDrawer(List<Type> types, Type defaultType) {
            typeSelector = new FormattedTypeSelector(types, supportsMultiSelect: false);
            targetType = defaultType;
            typeSelector.SelectionChanged += types => {
                targetType = types.FirstOrDefault();
            };
        }

        public void ResetSelectorTypes(List<Type> types) {
            typeSelector = new FormattedTypeSelector(types, supportsMultiSelect: false);
        }

        public OdinSelector<Type> SelectType(Rect _) {
            typeSelector.SetSelection(targetType);
            typeSelector.ShowInPopup(new Rect(-300f, 0f, 300f, 0f));
            return typeSelector;
        }
        //EditorGUIUtility.labelWidth

        public Type TypeField() {
            var typeText = targetType == null ? "Select Type" : targetType.GetNiceFullName();
            var typeTextContent = new GUIContent(typeText);
            var typeTextStyle = EditorStyles.layerMaskField;
            var rect = GUILayoutUtility.GetRect(typeTextContent, typeTextStyle);
            var typeLabelRect = rect.SetSize(EditorGUIUtility.labelWidth, rect.height);
            var typeSelectorRect = new Rect(rect.x + EditorGUIUtility.labelWidth + 2, rect.y, Max(rect.width - EditorGUIUtility.labelWidth - 2, 0), rect.height);
            EditorGUI.LabelField(typeLabelRect, GUIHelper.TempContent("Type"));
            OdinSelector<Type>.DrawSelectorDropdown(typeSelectorRect, typeTextContent, SelectType, typeTextStyle);
            return targetType;
        }
    }
}
