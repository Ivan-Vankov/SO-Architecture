using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;

namespace Vaflov {
    [DrawerPriority(0.0, 0.0, 2003.0)]
    public class AnimParamAttributeDrawer : OdinAttributeDrawer<AnimParamAttribute> {
        private string error;

        private ValueResolver<Animator> animatorGetter;

        public AnimatorController animatorController;

        //private LocalPersistentContext<bool> isToggled;

        //
        // Summary:
        //     Initializes this instance.
        protected override void Initialize() {
            animatorGetter = ValueResolver.Get<Animator>(Property, Attribute.animator);
            //isToggled = this.GetPersistentValue("Toggled", SirenixEditorGUI.ExpandFoldoutByDefault);
            error = animatorGetter.ErrorMessage;
        }

        //
        // Summary:
        //     Draws the property with GUILayout support. This method is called by DrawPropertyImplementation
        //     if the GUICallType is set to GUILayout, which is the default.
        protected override void DrawPropertyLayout(GUIContent label) {
            if (Property.ValueEntry == null) {
                CallNextDrawer(label);
                return;
            }
            if (error != null) {
                SirenixEditorGUI.ErrorMessageBox(error);
                CallNextDrawer(label);
                return;
            }
            var animator = animatorGetter.GetValue();
            if (animator == null) {
                SirenixEditorGUI.ErrorMessageBox("Animator is null");
                CallNextDrawer(label);
                return;
            }
            animatorController = animator.runtimeAnimatorController as AnimatorController;
            if (animatorController == null) {
                SirenixEditorGUI.ErrorMessageBox("Animator has no animation controller");
                CallNextDrawer(label);
                return;
            }

            var currentParamName = Property.ValueEntry.WeakSmartValue?.ToString();
            var seen = false;
            foreach (var param in animatorController.parameters) {
                if (param.type == Attribute.type && param.name == currentParamName) {
                    seen = true;
                }
            }
            if (!seen) {
                SirenixEditorGUI.ErrorMessageBox("Param name not found in animator controller");
            }
            var valueLabel = GUIHelper.TempContent(currentParamName);
            OdinSelector<string>.DrawSelectorDropdown(label, valueLabel, ShowSelector);
        }

        private OdinSelector<string> ShowSelector(Rect rect) {
            GenericSelector<string> animParamSelector = CreateSelector();
            rect.x = (int)rect.x;
            rect.y = (int)rect.y;
            rect.width = (int)rect.width;
            rect.height = (int)rect.height;

            animParamSelector.SelectionConfirmed += x => {
                Property.ValueEntry.WeakSmartValue = x.FirstOrDefault();
            };

            animParamSelector.ShowInPopup(rect);
            return animParamSelector;
        }

        private GenericSelector<string> CreateSelector() {
            var paramNames = Array.Empty<string>();
            if (animatorController) {
                paramNames = animatorController.parameters
                    .Where(param => param.type == Attribute.type)
                    .Select(param => param.name)
                    .ToArray();
            }
            GenericSelector<string> animParamSelector = new GenericSelector<string>(null, paramNames, false);
            
            animParamSelector.SelectionTree.Config.DrawSearchToolbar = paramNames.Length > 5;
            animParamSelector.EnableSingleClickToSelect();
            animParamSelector.SelectionTree.SortMenuItemsByName();

            return animParamSelector;
        }
    }
}
