#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;

namespace Vaflov {
    public class ClampedConstantPropertyProcessor<C, T> : OdinPropertyProcessor<C>, IDisposable
        where C : ClampedConstant<T>
        where T : IComparable, IComparable<T>, IEquatable<T> {

        protected override void Initialize() {
            ClampedConstantEditorEvents.OnConstantClampedChanged += RefreshGUI;
        }

        public void Dispose() {
            ClampedConstantEditorEvents.OnConstantClampedChanged -= RefreshGUI;
        }

        public void RefreshGUI() {
            Property.RefreshSetup();
            GUIHelper.RequestRepaint();
        }

        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos) {
            var constant = ValueEntry.SmartValue;
            var valueProp = propertyInfos.Find("value");
            if (constant.clamped) {
                valueProp.GetEditableAttributesList().Add(new PropertyRangeAttribute("min", "max"));
            }
        }
    }
}
#endif