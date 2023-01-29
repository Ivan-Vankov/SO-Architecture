using System.Diagnostics;
using System;

namespace Vaflov {
    //
    // Summary:
    //     DrawButtonTypes is used together with the Button attribute on a method
    //     to show a "Show Types" toggle on the drawn button. That toggles shows the
    //     method parameter types.
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public class DrawButtonTypesAttribute : Attribute {
        public bool drawTypesState = false;
    }
}
