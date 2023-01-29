#if ODIN_INSPECTOR && UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using UnityEngine;

namespace Vaflov {
    public class StringConstantPropertyProcessor<C> : OdinPropertyProcessor<C>  where C : Constant<string> {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos) {
            propertyInfos.Find("value").GetEditableAttributesList()
                .Add(new TextAreaAttribute(1, 15));
        }
    }
}
#endif