using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Vaflov {
    public class BoolConstant : Constant<bool> {
        [Button]
        public void Test() {
            var ob = typeof(Material);
            TypeCache.GetTypesDerivedFrom(typeof(Constant<>))
                .ForEach(t => {
                    if (t.IsGenericType && t.GetGenericArguments()[0].GenericParameterIsFulfilledBy(ob)) {
                        //Debug.Log(t.Name.Substring(0, t.Name.IndexOf('`')));
                        Debug.Log(t.Name.Remove(t.Name.IndexOf('`')));
                    }
                });
                //.Where(type => {
                //    if (type.IsGenericType)
                //        return false;
                //    return type.BaseType.GenericTypeArguments[0] == wrappedConstantType;
                //    //type.GenericArgumentsContainsTypes(wrappedConstantType);
                //    //type.GenericParameterIsFulfilledBy
                //});
        }
    }
}
