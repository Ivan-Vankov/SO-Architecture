#if ODIN_INSPECTOR && UNITY_EDITOR
using System;
using System.CodeDom;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ActionResolvers;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Vaflov {
    [DrawerPriority(0.0, 0.0, 0.12)]
    public sealed class TypeDecoratedMethodDrawer : MethodDrawer {
        [ShowOdinSerializedPropertiesInInspector]
        private class MethodResultInspector {
            [HideReferenceObjectPicker]
            [HideLabel]
            public object Value;
        }

        public static readonly CSharpCodeProvider codeProvider = new CSharpCodeProvider();
        internal static bool DontDrawMethodParameters = false;

        private bool drawParameters;

        private bool hasReturnValue;

        private bool shouldDrawResult;

        private string name;

        private ButtonAttribute buttonAttribute;

        private DrawButtonTypesAttribute drawTypesIfAttribute;

        private GUIStyle style;

        private GUIStyle toggleBtnStyle;

        private ValueResolver<string> labelGetter;

        private GUIContent label;

        private ButtonStyle btnStyle;

        private bool expanded;

        private Color btnColor;

        private bool hasGUIColorAttribute;

        private bool hasInvokedOnce;

        private ActionResolver buttonActionResolver;

        private ValueResolver<object> buttonValueResolver;

        private bool hideLabel;

        private string tooltip;

        private int buttonHeight;

        private float buttonAlignment;

        private IconAlignment buttonIconAlignment;

        private bool stretch;

        private bool drawnByGroup;

        private float previousFrameWidth;

        private static GUIStyle _sdfIconButtonLabelStyle;
        private static GUIStyle SdfIconButtonLabelStyle {
            get {
                if (_sdfIconButtonLabelStyle == null) {
                    _sdfIconButtonLabelStyle = new GUIStyle {
                        alignment = TextAnchor.MiddleCenter,
                        padding = new RectOffset(0, 0, 0, 0),
                        clipping = TextClipping.Clip
                    };
                }

                return _sdfIconButtonLabelStyle;
            }
        }

        protected override bool CanDrawMethodProperty(InspectorProperty property) {
            return property.Attributes.HasAttribute<DrawButtonTypesAttribute>();
        }

        //
        // Summary:
        //     Initializes this instance.
        protected override void Initialize() {
            expanded = false;
            buttonAttribute = base.Property.GetAttribute<ButtonAttribute>();
            drawTypesIfAttribute = base.Property.GetAttribute<DrawButtonTypesAttribute>();
            hasGUIColorAttribute = base.Property.GetAttribute<GUIColorAttribute>() != null;
            drawParameters = base.Property.Children.Count > 0 && !DontDrawMethodParameters && (buttonAttribute == null || buttonAttribute.DisplayParameters);
            hasReturnValue = base.Property.Children.Count > 0 && base.Property.Children[base.Property.Children.Count - 1].Name == "$Result";
            hideLabel = base.Property.Attributes.HasAttribute<HideLabelAttribute>() || buttonAttribute?.Name == string.Empty;
            tooltip = base.Property.GetAttribute<PropertyTooltipAttribute>()?.Tooltip;
            buttonHeight = base.Property.Context.GetGlobal("ButtonHeight", (buttonAttribute != null && buttonAttribute.HasDefinedButtonHeight) ? buttonAttribute.ButtonHeight : GlobalConfig<GeneralDrawerConfig>.Instance.ButtonHeight).Value;
            buttonHeight = Mathf.Max(buttonHeight, (int)EditorGUIUtility.singleLineHeight);
            buttonIconAlignment = base.Property.Context.GetGlobal("IconAlignment", (buttonAttribute != null && buttonAttribute.HasDefinedButtonIconAlignment) ? buttonAttribute.IconAlignment : GlobalConfig<GeneralDrawerConfig>.Instance.ButtonIconAlignment).Value;
            buttonAlignment = base.Property.Context.GetGlobal("ButtonAlignment", (buttonAttribute != null && buttonAttribute.HasDefinedButtonAlignment) ? buttonAttribute.ButtonAlignment : GlobalConfig<GeneralDrawerConfig>.Instance.ButtonAlignment).Value;
            stretch = base.Property.Context.GetGlobal("StretchButton", (buttonAttribute != null && buttonAttribute.HasDefinedStretch) ? buttonAttribute.Stretch : GlobalConfig<GeneralDrawerConfig>.Instance.StretchButtons).Value;
            style = base.Property.Context.GetGlobal("ButtonStyle", (buttonHeight > 20) ? SirenixGUIStyles.Button : EditorStyles.miniButton).Value;
            drawnByGroup = base.Property.Context.GetGlobal("DrawnByGroup", defaultValue: false).Value;
            shouldDrawResult = GlobalConfig<GeneralDrawerConfig>.Instance.ShowButtonResultsByDefault;
            if (buttonAttribute != null) {
                if (!buttonAttribute.DisplayParameters) {
                    if (hasReturnValue) {
                        buttonValueResolver = ValueResolver.Get<object>(base.Property, null);
                    } else {
                        buttonActionResolver = ActionResolver.Get(base.Property, null);
                    }
                }

                btnStyle = buttonAttribute.Style;
                expanded = buttonAttribute.Expanded;
                if (!string.IsNullOrEmpty(buttonAttribute.Name)) {
                    labelGetter = ValueResolver.GetForString(base.Property, buttonAttribute.Name);
                }

                if (buttonAttribute.DrawResultIsSet) {
                    shouldDrawResult = buttonAttribute.DrawResult;
                }
            }

            if (!shouldDrawResult && hasReturnValue && base.Property.Children.Count == 1) {
                drawParameters = false;
            }

            if (drawParameters && btnStyle == ButtonStyle.FoldoutButton && !expanded) {
                if (buttonHeight > 20) {
                    style = SirenixGUIStyles.ButtonLeft;
                    toggleBtnStyle = SirenixGUIStyles.ButtonRight;
                } else {
                    style = EditorStyles.miniButtonLeft;
                    toggleBtnStyle = EditorStyles.miniButtonRight;
                }
            }
            Property.State.Expanded = true;
        }

        //
        // Summary:
        //     Draws the property layout.
        protected override void DrawPropertyLayout(GUIContent lbl) {
            if (buttonActionResolver != null && buttonActionResolver.HasError) {
                buttonActionResolver.DrawError();
            }

            if (buttonValueResolver != null && buttonValueResolver.HasError) {
                buttonValueResolver.DrawError();
            }

            labelGetter?.DrawError();
            label = new GUIContent(hideLabel ? "" : ((labelGetter != null) ? labelGetter.GetValue() : base.Property.NiceName));
            label.tooltip = tooltip ?? label.text;
            base.Property.Label = label;
            btnColor = GUI.color;
            //Color color = (hasGUIColorAttribute ? GUIColorAttributeDrawer.CurrentOuterColor : btnColor);
            Color color = btnColor;
            GUIHelper.PushColor(color);
            int value = base.Property.Context.GetGlobal("ButtonHeight", 0).Value;
            GUIStyle value2 = base.Property.Context.GetGlobal("ButtonStyle", (GUIStyle)null).Value;
            if ((buttonHeight != value && value != 0) || (value2 != null && style != value2)) {
                Initialize();
            }

            if (!drawParameters) {
                DrawNormalButton();
            } else if (btnStyle == ButtonStyle.FoldoutButton) {
                if (expanded) {
                    DrawNormalButton();
                    EditorGUI.indentLevel++;
                    DrawParameters(appendButton: false);
                    EditorGUI.indentLevel--;
                } else {
                    DrawFoldoutButton();
                }
            } else if (btnStyle == ButtonStyle.CompactBox) {
                DrawCompactBoxButton();
            } else if (btnStyle == ButtonStyle.Box) {
                DrawBoxButton();
            }

            GUIHelper.PopColor();
        }

        private void DrawBoxButton() {
            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.BeginToolbarBoxHeader();
            if (expanded) {
                EditorGUILayout.LabelField(label);
            } else {
                base.Property.State.Expanded = SirenixEditorGUI.Foldout(base.Property.State.Expanded, label);
            }

            SirenixEditorGUI.EndToolbarBoxHeader();
            DrawParameters(appendButton: true);
            SirenixEditorGUI.EndToolbarBox();
        }

        private void DrawCompactBoxButton() {
            SirenixEditorGUI.BeginBox();
            var toggleWidth = 90;
            var invokeWidth = buttonAttribute.HasDefinedIcon ? 90 : 70;
            var padding = 3;
            Rect rect = SirenixEditorGUI.BeginBoxHeader().AlignRight(toggleWidth + invokeWidth + padding + 4).Padding(1f);
            rect.height -= 2;
            rect.y += 1;

            var toggleRect = new Rect(rect.x, rect.y, toggleWidth, rect.height);
            var invokeRect = new Rect(rect.x + toggleWidth + padding, rect.y, invokeWidth, rect.height);

            GUIHelper.PushColor(btnColor);
            drawTypesIfAttribute.drawTypesState = SirenixEditorGUIUtil.ToggleButton(toggleRect, "Show Types", drawTypesIfAttribute.drawTypesState);
            var content = new GUIContent(hideLabel ? "" : "Invoke", tooltip);
            if (buttonAttribute.HasDefinedIcon) {
                if (SirenixEditorGUI.SDFIconButton(invokeRect, content, buttonAttribute.Icon, buttonIconAlignment, SirenixGUIStyles.MiniButton)) {
                    InvokeButton();
                }
            } else if (GUI.Button(invokeRect, content, SirenixGUIStyles.MiniButton)) {
                InvokeButton();
            }

            GUIHelper.PopColor();
            if (expanded) {
                EditorGUILayout.LabelField(label);
            } else {
                Property.State.Expanded = SirenixEditorGUI.Foldout(drawTypesIfAttribute.drawTypesState || Property.State.Expanded, label);
            }

            SirenixEditorGUI.EndBoxHeader();
            DrawParameters(appendButton: false);
            SirenixEditorGUI.EndBox();
        }

        private void DrawNormalButton() {
            bool flag = buttonAttribute != null && buttonAttribute.HasDefinedIcon;
            //float num = SirenixEditorGUI.CalculateMinimumSDFIconButtonWidth(label, buttonHeight);
            SirenixEditorGUI.CalculateMinimumSDFIconButtonWidth(label?.text, style, buttonAttribute?.HasDefinedIcon ?? false, buttonHeight, out var _, out var _, out var _, out var totalWidth);
            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, style, stretch ? GUILayoutOptions.Height(buttonHeight) : GUILayoutOptions.Height(buttonHeight).MaxWidth(totalWidth));
            if (!stretch && !drawnByGroup) {
                Rect controlRect = EditorGUILayout.GetControlRect(true, 0f);
                GUILayout.Space(-2f);
                if (Event.current.type == EventType.Repaint) {
                    previousFrameWidth = controlRect.width;
                }

                rect = rect.SetX(Mathf.Clamp(rect.x + buttonAlignment * previousFrameWidth - totalWidth / 2f, rect.x, rect.x + previousFrameWidth - totalWidth));
            }

            rect = EditorGUI.IndentedRect(rect);
            GUIHelper.PushColor(btnColor);
            if (flag) {
                if (SirenixEditorGUI.SDFIconButton(rect, label, buttonAttribute.Icon, buttonIconAlignment, style)) {
                    InvokeButton();
                }
            } else if (GUI.Button(rect, label, style)) {
                InvokeButton();
            }

            GUIHelper.PopColor();
        }

        private void DrawFoldoutButton() {
            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, style, GUILayoutOptions.Height(buttonHeight));
            rect = EditorGUI.IndentedRect(rect);
            GUIHelper.PushColor(btnColor);
            Rect rect2 = rect.AlignRight(20f);
            if (GUI.Button(rect2, GUIContent.none, toggleBtnStyle)) {
                base.Property.State.Expanded = !base.Property.State.Expanded;
            }

            rect.width -= rect2.width;
            if (!base.Property.State.Expanded) {
                rect2.x -= 1f;
                rect2.yMin -= 1f;
            }

            if (base.Property.State.Expanded) {
                EditorIcons.TriangleDown.Draw(rect2, 16f);
            } else {
                EditorIcons.TriangleLeft.Draw(rect2, 16f);
            }

            if (buttonAttribute != null && buttonAttribute.HasDefinedIcon) {
                if (SirenixEditorGUI.SDFIconButton(rect, label, buttonAttribute.Icon, buttonIconAlignment, style)) {
                    InvokeButton();
                }
            } else if (GUI.Button(rect, label, style)) {
                InvokeButton();
            }

            GUIHelper.PopColor();
            EditorGUI.indentLevel++;
            DrawParameters(appendButton: false);
            EditorGUI.indentLevel--;
        }

        private void DrawParameters(bool appendButton) {
            if (SirenixEditorGUI.BeginFadeGroup(this, base.Property.State.Expanded || expanded)) {
                GUILayout.Space(0f);
                for (int i = 0; i < base.Property.Children.Count; i++) {
                    bool flag = false;
                    if (hasReturnValue && i == base.Property.Children.Count - 1) {
                        if (!shouldDrawResult || (!hasInvokedOnce && i != 0)) {
                            break;
                        }

                        if (i != 0) {
                            SirenixEditorGUI.DrawThickHorizontalSeparator();
                        }

                        flag = true;
                    }

                    if (flag && !hasInvokedOnce) {
                        GUIHelper.PushGUIEnabled(enabled: false);
                    }

                    var child = base.Property.Children[i];
                    if (drawTypesIfAttribute.drawTypesState) {
                        SirenixEditorGUI.BeginBox();
                        var typeStr = codeProvider.GetTypeOutput(new CodeTypeReference(child.ValueEntry.TypeOfValue));
                        GUIHelper.PushGUIEnabled(enabled: false);
                        EditorGUILayout.LabelField(typeStr);
                        GUIHelper.PopGUIEnabled();
                    }
                    child.Draw();
                    if (drawTypesIfAttribute.drawTypesState) {
                        SirenixEditorGUI.EndBox();
                    }

                    if (flag && !hasInvokedOnce) {
                        GUIHelper.PopGUIEnabled();
                    }
                }

                if (appendButton) {
                    Rect rect = EditorGUILayout.BeginVertical(SirenixGUIStyles.BottomBoxPadding).Expand(3f);
                    SirenixEditorGUI.DrawHorizontalLineSeperator(rect.x, rect.y, rect.width);
                    DrawNormalButton();
                    EditorGUILayout.EndVertical();
                }
            }

            SirenixEditorGUI.EndFadeGroup();
        }

        private void InvokeButton() {
            try {
                bool flag = hasReturnValue && Event.current.button == 1;
                GUIHelper.RemoveFocusControl();
                GUIHelper.RequestRepaint();
                if (((base.Property.Info.GetMemberInfo() as MethodInfo) ?? base.Property.Info.GetMethodDelegate().Method).IsGenericMethodDefinition) {
                    Debug.LogError("Cannot invoke a generic method definition.");
                    return;
                }

                if (buttonAttribute == null || buttonAttribute.DirtyOnClick) {
                    if (base.Property.ParentValueProperty != null) {
                        base.Property.ParentValueProperty.RecordForUndo("Clicked Button '" + base.Property.NiceName + "'", forceCompleteObjectUndo: true);
                    }

                    foreach (UnityEngine.Object item in base.Property.SerializationRoot.ValueEntry.WeakValues.OfType<UnityEngine.Object>()) {
                        InspectorUtilities.RegisterUnityObjectDirty(item);
                    }
                }

                if (buttonActionResolver != null) {
                    buttonActionResolver.DoActionForAllSelectionIndices();
                } else if (buttonValueResolver != null) {
                    for (int i = 0; i < base.Property.Tree.WeakTargets.Count; i++) {
                        object value = buttonValueResolver.GetValue(i);
                        base.Property.Children[base.Property.Children.Count - 1].ValueEntry.WeakValues[i] = value;
                    }
                } else {
                    MethodInfo methodInfo = (MethodInfo)base.Property.Info.GetMemberInfo();
                    if (methodInfo != null) {
                        InvokeMethodInfo(methodInfo);
                    } else {
                        InvokeDelegate();
                    }
                }

                if (flag) {
                    object weakSmartValue = base.Property.Children[base.Property.Children.Count - 1].ValueEntry.WeakSmartValue;
                    OdinEditorWindow.InspectObjectInDropDown(new MethodResultInspector {
                        Value = weakSmartValue
                    });
                }
            } finally {
                GUIUtility.ExitGUI();
            }
        }

        private void InvokeDelegate() {
            try {
                int num = (hasReturnValue ? (base.Property.Children.Count - 1) : base.Property.Children.Count);
                object[] array = new object[num];
                for (int i = 0; i < array.Length; i++) {
                    array[i] = base.Property.Children[i].ValueEntry.WeakSmartValue;
                }

                object weakSmartValue = base.Property.Info.GetMethodDelegate().DynamicInvoke(array);
                for (int j = 0; j < array.Length; j++) {
                    base.Property.Children[j].ValueEntry.WeakSmartValue = array[j];
                }

                if (hasReturnValue) {
                    base.Property.Children[base.Property.Children.Count - 1].ValueEntry.WeakSmartValue = weakSmartValue;
                }

                if (!hasInvokedOnce) {
                    base.Property.Tree.DelayActionUntilRepaint(delegate {
                        hasInvokedOnce = true;
                    });
                }
            } catch (TargetInvocationException ex) {
                if (ex.IsExitGUIException()) {
                    throw ex.AsExitGUIException();
                }

                Debug.LogException(ex);
            } catch (ExitGUIException ex2) {
                throw ex2;
            } catch (Exception ex3) {
                if (ex3.IsExitGUIException()) {
                    throw ex3.AsExitGUIException();
                }

                Debug.LogException(ex3);
            }
        }

        private void InvokeMethodInfo(MethodInfo methodInfo) {
            InspectorProperty parentValueProperty = base.Property.ParentValueProperty;
            ImmutableList parentValues = base.Property.ParentValues;
            int num = (hasReturnValue ? (base.Property.Children.Count - 1) : base.Property.Children.Count);
            for (int i = 0; i < parentValues.Count; i++) {
                object obj = parentValues[i];
                if (obj == null && !methodInfo.IsStatic) {
                    continue;
                }

                try {
                    object[] array = new object[num];
                    for (int j = 0; j < array.Length; j++) {
                        array[j] = base.Property.Children[j].ValueEntry.WeakSmartValue;
                    }

                    object weakSmartValue = ((!methodInfo.IsStatic) ? methodInfo.Invoke(obj, array) : methodInfo.Invoke(null, array));
                    for (int k = 0; k < array.Length; k++) {
                        base.Property.Children[k].ValueEntry.WeakSmartValue = array[k];
                    }

                    if (hasReturnValue) {
                        base.Property.Children[base.Property.Children.Count - 1].ValueEntry.WeakSmartValue = weakSmartValue;
                    }

                    if (!hasInvokedOnce) {
                        base.Property.Tree.DelayActionUntilRepaint(delegate {
                            hasInvokedOnce = true;
                        });
                    }
                } catch (TargetInvocationException ex) {
                    if (ex.IsExitGUIException()) {
                        throw ex.AsExitGUIException();
                    }

                    Debug.LogException(ex);
                } catch (ExitGUIException ex2) {
                    throw ex2;
                } catch (Exception ex3) {
                    if (ex3.IsExitGUIException()) {
                        throw ex3.AsExitGUIException();
                    }

                    Debug.LogException(ex3);
                }

                if (parentValueProperty != null && obj.GetType().IsValueType) {
                    parentValueProperty.ValueEntry.WeakValues[i] = obj;
                }
            }
        }
    }
}
#endif