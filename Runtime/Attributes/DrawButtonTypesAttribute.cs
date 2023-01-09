using System.Diagnostics;
using System;

namespace Vaflov {
    //
    // Summary:
    //     TODO: DrawTypes is used ...
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public class DrawButtonTypesAttribute : Attribute {
        public bool drawTypesState = false;
    }
}
