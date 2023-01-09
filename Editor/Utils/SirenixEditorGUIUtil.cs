#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vaflov {
    public static class SirenixEditorGUIUtil {
        public static bool ToolbarSDFIconButton(SdfIconType sdfIconType, int toolbarHeight, string text = null, string tooltip = null) {
            return ToolbarSDFIconButton(sdfIconType, toolbarHeight, GUIHelper.TempContent(text, tooltip));
        }

        public static bool ToolbarSDFIconButton(SdfIconType sdfIconType, int toolbarHeight, GUIContent guiContent) {
            var rect = GUILayoutUtility.GetRect(toolbarHeight, 0f, GUILayoutOptions.ExpandWidth(expand: false).ExpandHeight());
            return SirenixEditorGUI.SDFIconButton(rect, guiContent, sdfIconType, style: SirenixGUIStyles.ToolbarButton);
        }

        public static bool ToolbarButton(EditorIcon editorIcon, int toolbarHeight, string text = null, string tooltip = null) {
            return ToolbarButton(editorIcon, toolbarHeight, GUIHelper.TempContent(text, tooltip));
        }

        public static bool ToolbarButton(EditorIcon editorIcon, int toolbarHeight, GUIContent guiContent) {
            Rect addRect = GUILayoutUtility.GetRect(toolbarHeight, 0f, GUILayoutOptions.ExpandWidth(expand: false).ExpandHeight());
            if (GUI.Button(addRect, guiContent, SirenixGUIStyles.ToolbarButton)) {
                GUIHelper.RemoveFocusControl();
                GUIHelper.RequestRepaint();
                return true;
            }

            if (Event.current.type == EventType.Repaint) {
                editorIcon.Draw(addRect, 16f);
            }
            return false;
        }

        public static bool ToggleButton(string title, bool toggled) {
            GUIStyle guiStyle = SirenixGUIStyles.MiniButton;
            var guiContent = GUIHelper.TempContent(title);
            var rect = GUILayoutUtility.GetRect(guiContent, guiStyle);
            return ToggleButton(rect, guiContent, toggled, guiStyle);
        }

        public static bool ToggleButton(Rect rect, string title, bool toggled) {
            return ToggleButton(rect, GUIHelper.TempContent(title), toggled, SirenixGUIStyles.MiniButton);
        }

        public static bool ToggleButton(Rect rect, GUIContent guiContent, bool toggled) {
            return ToggleButton(rect, guiContent, toggled, SirenixGUIStyles.MiniButton);
        }

        public static bool ToggleButton(Rect rect, GUIContent guiContent, bool toggled, GUIStyle guiStyle) {
            if (GUI.Button(rect, GUIContent.none)) {
                GUIHelper.RemoveFocusControl();
                toggled = !toggled;
                GUIHelper.RequestRepaint();
            }

            if (Event.current.type == EventType.Repaint) {
                bool isHover = rect.Contains(Event.current.mousePosition);
                guiStyle.Draw(rect, guiContent, isHover, isActive: false, toggled, hasKeyboardFocus: false);
            }
            return toggled;
        }
    }
}
#endif