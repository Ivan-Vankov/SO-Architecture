#if ODIN_INSPECTOR
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Vaflov {
    public static class SirenixEditorGUIUtil {
        public static bool ToolbarButton(EditorIcon editorIcon, int toolbarHeight, string text = null, string tooltip = null) {
            return ToolbarButton(editorIcon, toolbarHeight, new GUIContent(text, tooltip));
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
    }
}
#endif