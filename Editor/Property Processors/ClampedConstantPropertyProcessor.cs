#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vaflov {
    public class ClampedConstantPropertyProcessor<C, T> : OdinPropertyProcessor<C>
        where C : ClampedConstant<T>
        where T : IComparable, IComparable<T>, IEquatable<T> {

        public static bool oldIsClamped;

        protected override void Initialize() {
            oldIsClamped = ValueEntry.SmartValue.isClamped;
            ValueEntry.OnChildValueChanged -= RefreshOnClamped;
            ValueEntry.OnChildValueChanged += RefreshOnClamped;
        }

        public void RefreshOnClamped(int _) {
            var isClamped = ValueEntry.SmartValue.isClamped;
            if (oldIsClamped != isClamped) {
                Property.RefreshSetup();
                GUIHelper.RequestRepaint();
            }
            oldIsClamped = isClamped;
        }

        public static (int i, InspectorPropertyInfo propInfo) FindProp(List<InspectorPropertyInfo> infos, string name) {
            for (int i = 0; i < infos.Count; i++) {
                if (infos[i].PropertyName == name) {
                    return (i, infos[i]);
                }
            }
            return (-1, null);
        }

        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos) {
            var constant = ValueEntry.SmartValue;
            (var valuePropIdx, var valueProp) = FindProp(propertyInfos, "value");
            (var minPropIdx, var minProp) = FindProp(propertyInfos, "min");
            var maxProp = propertyInfos.Find("max");
            if (constant.isClamped) {
                valueProp.GetEditableAttributesList().Add(new PropertyRangeAttribute("min", "max"));

                var minAttributes = minProp.GetEditableAttributesList();
                minAttributes.Add(new LabelWidthAttribute(30));
                minAttributes.Add(new HorizontalGroupAttribute("Slider"));

                propertyInfos.RemoveAt(minPropIdx);
                propertyInfos.Insert(valuePropIdx, minProp);

                var maxAttributes = maxProp.GetEditableAttributesList();
                maxAttributes.Add(new LabelWidthAttribute(30));
                maxAttributes.Add(new HorizontalGroupAttribute("Slider"));
            } else {
                minProp.GetEditableAttributesList().Add<HideInInspector>();
                maxProp.GetEditableAttributesList().Add<HideInInspector>();
            }
        }
    }
}
#endif