#if ODIN_INSPECTOR && UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;

namespace Vaflov {
    public class RangeConstantPropertyProcessor<C, T, U> : ReloadingPropertyProcessor<C>, IDisposable
        where C : RangeConstant<T, U> {
        public override ref Action ReloadAction => ref RangeConstant.OnConstantRangeChanged;

        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos) {
            var constant = ValueEntry.SmartValue;
            if (constant.isRange) {
                var valueAttributes = propertyInfos.Find("value").GetEditableAttributesList();
                valueAttributes.Add(new MinMaxSliderAttribute("min", "max", true));
            }
        }
    }
}
#endif