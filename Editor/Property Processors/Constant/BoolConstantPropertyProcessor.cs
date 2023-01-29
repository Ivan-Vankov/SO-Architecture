#if ODIN_INSPECTOR && UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using static Vaflov.Config;

namespace Vaflov {
    public class BoolConstantPropertyProcessor<T> : OdinPropertyProcessor<T> where T : Constant<bool> {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos) {
            var valueAttributes = propertyInfos.Find("value").GetEditableAttributesList();
            valueAttributes.RemoveAttributeOfType<HideLabelAttribute>();
            valueAttributes.Add(new LabelWidthAttribute(preferedEditorLabelWidth));
        }
    }
}
#endif