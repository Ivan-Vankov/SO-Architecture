using System.Diagnostics;
using System;

namespace Vaflov {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public class RuntimeSetObjectPickerAttribute : Attribute {
        public string type;

        public RuntimeSetObjectPickerAttribute(string type) {
            this.type = type;
        }
    }
}
