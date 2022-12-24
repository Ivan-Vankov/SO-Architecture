using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;

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
            var constant = ValueEntry.SmartValue;
            var valueProp = propertyInfos.Find("value");
            var valueAttributes = valueProp.GetEditableAttributesList();
            valueAttributes.Add(new AssetsOnlyAttribute());
            switch (constant.preview) {
                case ObjectPreviewKind.InlineEditor:
                    valueAttributes.Add(new InlineEditorAttribute(constant.editorMode));
                    break;
                case ObjectPreviewKind.PreviewField:
                    valueAttributes.Add(new PreviewFieldAttribute(constant.alignment));
                    break;
                case ObjectPreviewKind.None:
                default: break;
            }
        }
    }
}
