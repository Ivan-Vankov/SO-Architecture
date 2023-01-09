using System;

namespace Vaflov {
    //
    // Summary:
    //     CodegenInapplicable is used to skip a class when codegen searches for target classes.
    [AttributeUsage(AttributeTargets.Class)]
    public class CodegenInapplicableAttribute : Attribute { }
}
