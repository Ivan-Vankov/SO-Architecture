#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;

namespace Vaflov {
    public class RangeConstantPropertyProcessor<C, T> : OdinPropertyProcessor<C>, IDisposable
        where C : RangeConstant<T> {

        protected override void Initialize() {
            RangeConstantEditorEvents.OnConstantRangeChanged += RefreshGUI;
        }

        public void Dispose() {
            RangeConstantEditorEvents.OnConstantRangeChanged -= RefreshGUI;
        }

        public void RefreshGUI() {
            Property.RefreshSetup();
            GUIHelper.RequestRepaint();
        }

        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos) {
            var constant = ValueEntry.SmartValue;
            if (constant.isRange) {
                var valueAttributes = propertyInfos.Find("value").GetEditableAttributesList();
                valueAttributes.Add(new MinMaxSliderAttribute("range", true));
            }
        }
    }
}
#endif