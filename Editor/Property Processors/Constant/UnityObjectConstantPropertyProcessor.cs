#if ODIN_INSPECTOR && UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vaflov {
    public class UnityObjectConstantPropertyProcessor<T, C> : ReloadingPropertyProcessor<T>, IDisposable
        where T : UnityObjectConstant<C>
        where C : UnityEngine.Object {
        public override ref Action ReloadAction => ref UnityObjectConstant.OnUnityObjectConstantChanged;

        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos) {
            var valueAttributes = propertyInfos.Find("value").GetEditableAttributesList();
            valueAttributes.Add(new AssetsOnlyAttribute());
            //valueAttributes.Add(new SerializeReference());
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
#endif