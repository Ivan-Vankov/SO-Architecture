using System.Diagnostics;
using System;
using UnityEngine;

namespace Vaflov {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public class RuntimeSetObjectPickerAttribute : Attribute {
        public string Type;

        public RuntimeSetObjectPickerAttribute(string type) {
            this.Type = type;
        }
    }
}
