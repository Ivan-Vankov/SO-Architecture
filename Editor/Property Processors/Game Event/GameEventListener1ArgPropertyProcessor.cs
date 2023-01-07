#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using ExtEvents;
using UnityEngine;

namespace Vaflov {
    // TODO: Remove this
    public class GameEventListener1ArgPropertyProcessor<C, T> : OdinPropertyProcessor<C> where C : GameEventListener1Arg<T> {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos) {
            //var responseProp = propertyInfos.Find(nameof(GameEventListener1Arg<T>.response));
            //var argName = ValueEntry.SmartValue.eventRef.argName;
            //Debug.Log(argName);
            //responseProp.GetEditableAttributesList().Add(new EventArgumentsAttribute(argName));
        }
    }
}
#endif