using Microsoft.CSharp;
using System;

namespace Vaflov {
    public static partial class Config {
        public const string PACKAGE_NAME = "SO Architecture";
        public const int preferedEditorLabelWidth = 70;
        public static readonly CSharpCodeProvider codeProvider = new CSharpCodeProvider();
        public static string AUTO_GENERATED_HEADER =
            "////////////////////////////////////////////////////////////////////" + Environment.NewLine
          + "/////////////////// AUTOMATICALLY GENERATED FILE ///////////////////" + Environment.NewLine
          + "////////////////////////////////////////////////////////////////////" + Environment.NewLine;
    }
}