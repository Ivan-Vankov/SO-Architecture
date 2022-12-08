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
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using System;
using TypeReferences;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
            window.EnableAutomaticHeightAdjustment(600, retainInitialWindowPosition: true);
        }

        [HideInInspector]
        //[HideLabel]
        //[LabelWidth(40)]
        //[LabelText("Type")]
        public Type targetType;

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

        private OdinSelector<Type> SelectType(Rect arg) {
            var targetTypeFlags = AssemblyTypeFlags.GameTypes;
            TypeSelector typeSelector = new TypeSelector(targetTypeFlags, supportsMultiSelect: false);
            typeSelector.SelectionChanged += types => {
                Type type = types.FirstOrDefault();
                if (type != null) {
                    targetType = type;
                    //base.titleContent = new GUIContent(targetType.GetNiceName());
                }
            };
            typeSelector.SetSelection(targetType);
            typeSelector.ShowInPopup(new Rect(-300f, 0f, 300f, 0f));
            return typeSelector;
        }
        
        
        [OnInspectorGUI]
        private void OnInspectorGUI() {
            var typeText = targetType == null ? "Select Type" : targetType.GetNiceName();
            typeText = typeText.Length <= 25 ? typeText : typeText.Substring(0, 25) + "...";
            var typeTextContent = new GUIContent(typeText);
            //var typeTextStyle = EditorStyles.toolbarButton;
            var typeTextStyle = EditorStyles.toolbarDropDown;
            var rect = GUILayoutUtility.GetRect(typeTextContent, typeTextStyle);
            rect = rect.SetWidth(Mathf.Max(rect.width, maxSize.x));
            //Rect rect = GUILayoutUtility.GetRect(0f, 21f, SirenixGUIStyles.ToolbarBackground);
            //GUILayoutUtility.GetRect(typeText, EditorS)
            //Rect rect2 = rect.AlignRight(80f);
            //Rect rect3 = rect.SetXMax(rect2.xMin);
            OdinSelector<Type>.DrawSelectorDropdown(rect, typeTextContent, SelectType, typeTextStyle);



            if (targetType == null) {
                using (new EditorGUI.DisabledScope(true)) {
                    GUILayout.Button(new GUIContent("Create Asset", "test tooltip"));
                }
            } else if (GUILayout.Button("Create Asset")) {
                Debug.Log("here");

                //EditorIconsOverview.OpenEditorIconsOverview();

                //DestroyImmediate(this);
                //serializedObject.Dispose();
                //onGenericSOCreated?.Invoke();
            }
        }
    }
}