using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Vaflov {
    public class GameObjectPropertyProcessor : UnityObjectConstantPropertyProcessor<UnityObjectConstant<GameObject>, GameObject> {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos) {
            base.ProcessMemberProperties(propertyInfos);
            var valueAttributes = propertyInfos.Find("value").GetEditableAttributesList();
            valueAttributes.Add(new OnValueChangedAttribute("OnEditorPropChanged"));
            //[DelayedProperty]
            //[OnValueChanged(nameof(OnEditorPropChanged))]//
        }
    }
}
