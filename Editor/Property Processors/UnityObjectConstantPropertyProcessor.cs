using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vaflov {
    public class UnityObjectConstantPropertyProcessor<T, C> : OdinPropertyProcessor<T>, IDisposable
        where T : UnityObjectConstant<C>
        where C : UnityEngine.Object {

        protected override void Initialize() {
            UnityObjectConstantEditorEvents.OnUnityObjectConstantChanged += RefreshGUI;
        }

        public void Dispose() {
            UnityObjectConstantEditorEvents.OnUnityObjectConstantChanged -= RefreshGUI;
        }

        public void RefreshGUI() {
            Property.RefreshSetup();
            GUIHelper.RequestRepaint();
        }

        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos) {
            var valueAttributes = propertyInfos.Find("value").GetEditableAttributesList();
            valueAttributes.Add(new AssetsOnlyAttribute());
            valueAttributes.Add(new SerializeReference());
            var hideEditorMode = false;
            InlineEditorModes? editorMode = null;
            if (UnityObjectContantConfig.inlineEditorModesBlacklist.Contains(typeof(C))) {
                hideEditorMode = true;
            } else if (UnityObjectContantConfig.defaultInlineEditorModes.ContainsKey(typeof(C))) {
                editorMode = UnityObjectContantConfig.defaultInlineEditorModes[typeof(C)];
                hideEditorMode = true;
            } else {
                editorMode = ValueEntry.SmartValue.editorMode;
            }
            if (hideEditorMode) {
                propertyInfos.Find("editorMode").GetEditableAttributesList()
                    .Add(new HideInInspector());
            }
            if (editorMode.HasValue) {
                valueAttributes.Add(new InlineEditorAttribute(editorMode.Value) {
                    Expanded = true
                });
            }
        }
    }
}
