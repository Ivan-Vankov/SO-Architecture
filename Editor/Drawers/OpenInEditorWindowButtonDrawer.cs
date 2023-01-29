#if ODIN_INSPECTOR && UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Vaflov {
    public class OpenInEditorWindowButtonDrawer : OdinValueDrawer<OpenInEditorWindowButton> {
        public bool show = true;

        protected override void Initialize() {
            var stackFrames = new System.Diagnostics.StackTrace().GetFrames();
            foreach (var stackFrame in stackFrames) {
                var method = stackFrame.GetMethod();
                if (method.Name == "DrawEditors" && TypeUtil.IsInheritedFrom(method.DeclaringType, typeof(OdinEditorWindow))) {
                    show = false;
                    break;
                }
            }
        }

        protected override void DrawPropertyLayout(GUIContent label) {
            if (!show)
                return;
            if (GUILayout.Button(GUIHelper.TempContent("Open in Editor Window"))) {
                var obj = Property.Parent.ValueEntry.WeakSmartValue;
                foreach (var editorObjectMenuType in EditorObjectMenuHook.EditorObjectMenuTypes) {
                    var openMenus = Resources.FindObjectsOfTypeAll(editorObjectMenuType);
                    var menuWasOpen = openMenus.Length != 0;
                    var editorObjMenu = (menuWasOpen
                        ? openMenus[0]
                        : ScriptableObject.CreateInstance(editorObjectMenuType)) as EditorObjectMenuEditorWindow;
                    var editorMenuTargetType = editorObjMenu.EditorObjBaseType;
                    if (!menuWasOpen) {
                        UnityEngine.Object.DestroyImmediate(editorObjMenu);
                    }

                    if (TypeUtil.IsInheritedFrom(obj.GetType(), editorMenuTargetType)) {
                        var openMethod = editorObjectMenuType.GetMethod("Open", BindingFlags.Static | BindingFlags.Public);
                        EditorObjectMenuEditorWindow openedEditorMenu;
                        if (openMethod != null) {
                            openedEditorMenu = openMethod.Invoke(null, null) as EditorObjectMenuEditorWindow;
                        } else {
                            openedEditorMenu = EditorWindow.GetWindow(editorObjectMenuType) as EditorObjectMenuEditorWindow;
                        }
                        openedEditorMenu.TrySelectMenuItemWithObject(obj);
                        return;
                    }
                }
                Debug.LogError($"No editor window defined for {obj}");
            }
        }
    }
}
#endif