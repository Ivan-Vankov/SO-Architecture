//using System;
//using System.Linq;
//using TypeReferences.Editor.Drawers;
//using UnityDropdown.Editor;
//using Util;

//namespace Vaflov {
//        /// <summary>
//        /// A window that shows the type selection dropdown immediately after the creation,
//        /// and closes once the type is chosen.
//        /// </summary>
//        internal class OneTypeSelectionWindow : ITypeSelectionWindow {
//            private DropdownWindow _dropdownWindow;

//            public void OnCreate(Action<Type[]> onTypeSelected, string[] genericArgNames, Type[][] genericParamConstraints) {
//                var typeOptionsAttribute = new NonGenericAttribute(genericParamConstraints[0]);
//                var dropdownTree = GetDropdownTree(typeOptionsAttribute, onTypeSelected);
//                _dropdownWindow = dropdownTree.ShowAsContext(typeOptionsAttribute.DropdownHeight);
//            }

//            private DropdownMenu GetDropdownTree(NonGenericAttribute attribute, Action<Type[]> onTypeSelected) {
//                var parentDrawer = new TypeDropdownDrawer(null, attribute, null);
//                var dropdownItems = parentDrawer.GetDropdownItems().ToList();

//                var dropdownMenu = new DropdownMenu<Type>(dropdownItems, type => {
//                    _dropdownWindow.Close();
//                    onTypeSelected(new[] { type });
//                },
//                    TypeReferences.Editor.ProjectSettings.SearchbarMinItemsCount, true, attribute.ShowNoneElement);

//                if (attribute.ExpandAllFolders)
//                    dropdownMenu.ExpandAllFolders();

//                return dropdownMenu;
//            }
//        }
//    }
//}

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using TypeReferences;
using UnityEditor;
using UnityEngine;

namespace Vaflov {
    public class OneTypeSelectionWindow : OdinEditorWindow {
        //[MenuItem("Tools/One Type Selection Window")]
        //public static void CreateMultiTypeSelectionWindow() {
        //    var window = CreateInstance<OneTypeSelectionWindow>();
        //    window.Show();
        //}

        public static void ShowInPopup(float width) {
            //EditorWindow focusedWindow = EditorWindow.focusedWindow;
            var window = CreateInstance<OneTypeSelectionWindow>();


            Vector2 mousePosition = Event.current.mousePosition;
            Rect btnRect = new Rect(mousePosition.x, mousePosition.y, 1f, 1f);

            window.DefaultLabelWidth = 0.33f;
            window.DrawUnityEditorPreview = true;
            btnRect.position = GUIUtility.GUIToScreenPoint(btnRect.position);
            window.position = btnRect;

            window.ShowAsDropDown(btnRect, new Vector2(200, width));
            //window.ShowAuxWindow();
            //window.ShowModal();
            //window.ShowModalUtility();
            //window.ShowNotification(new GUIContent("notif"));
            //window.ShowPopup();
            //window.ShowTab();
            //window.ShowUtility();

            //window.Show();

            //window.position = btnRect;

            window.EnableAutomaticHeightAdjustment(600, retainInitialWindowPosition: true);

            //window.position = btnRect;
            //window.Show();
            //OdinEditorWindow odinEditorWindow = InspectObjectInDropDown(window, width);
            //InspectObject(window);
            //odinEditorWindow.OnClose
            //SetupWindow(odinEditorWindow, focusedWindow);
        }

        //[HideLabel]
        [LabelWidth(40)]
        public Type type;

        [LabelWidth(40)]
        public new string name;

        // This tooltip doesn't work
        //[Tooltip("Select a type")]
        //[DisableIf(nameof(IsSelectedTypeInvalid))]
        //[Button]
        //public void CreateAsset() {
        //    Debug.Log("Here");
        //}

        //public bool IsSelectedTypeInvalid() {
        //    return selectedType.Type == null;
        //}

        [OnInspectorGUI]
        private void OnInspectorGUI() {
            if (type == null) {
                using (new EditorGUI.DisabledScope(true)) {
                    GUILayout.Button(new GUIContent("Create Asset", "test tooltip"));
                }
            } else if (GUILayout.Button("Create Asset")) {
                Debug.Log("here");
                //DestroyImmediate(this);
                //serializedObject.Dispose();
                //onGenericSOCreated?.Invoke();
            }
        }
    }
}