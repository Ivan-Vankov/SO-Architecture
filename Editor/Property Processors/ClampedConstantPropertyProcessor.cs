#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;

namespace Vaflov {
    public class ClampedConstantPropertyProcessor<C, T> : ReloadingPropertyProcessor<C>, IDisposable
        where C : ClampedConstant<T>
        where T : IComparable, IComparable<T>, IEquatable<T> {

        public override ref Action ReloadAction => ref ClampedConstantEditorEvents.OnConstantClampedChanged;

        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos) {
            var constant = ValueEntry.SmartValue;
            if (constant.clamped) {
                var valueAttributes = propertyInfos.Find("value").GetEditableAttributesList();
                valueAttributes.Add(new PropertyRangeAttribute("min", "max"));
            }
        }
    }
}
#endif