#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System.Diagnostics;
using System;
using UnityEngine;

namespace Vaflov {
    //
    // Summary:
    //     TODO: DrawTypes is used ...
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public class DrawTypesIfAttribute : Attribute {
        public string Condition;

        public DrawTypesIfAttribute(string condition) {
            this.Condition = condition;
        }
    }
}
