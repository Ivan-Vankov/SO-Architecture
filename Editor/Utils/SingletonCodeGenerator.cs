using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using static Vaflov.FileUtil;

namespace Vaflov {
    public class SingletonCodeGenerator {
        public string singletonNamespaceName = typeof(SingletonCodeGenerator).Namespace;
        public string singletonClassModifiers = "";
        public string singletonClassName;
        public string singletonInstanceName = "Instance";
        public string singletonConceptName;
        public string singletonFieldSuffix;
        public string singletonDirectoryName = "SO Architecture";
        public StringBuilder singletonCodeBuilder = new StringBuilder();
        public System.Diagnostics.Stopwatch singletonCodegenTimer = new System.Diagnostics.Stopwatch();
        public string singletonCodePath = null;

        public delegate string NameFilter(string name, bool isFirstLetterLowerCase);
        public StringBuilder singletonNameBuilder = new StringBuilder();

        public SingletonCodeGenerator(string singletonClassName, string singletonConceptName) {
            this.singletonClassName = singletonClassName;
            this.singletonConceptName = singletonConceptName;
        }

        public SingletonCodeGenerator SetSingletonNamespaceName(string namespaceName) {
            this.singletonNamespaceName = namespaceName;
            return this;
        }

        public SingletonCodeGenerator SetSingletonClassModifiers(string singletonClassModifiers) {
            this.singletonClassModifiers = singletonClassModifiers;
            return this;
        }

        public SingletonCodeGenerator SetSingletonClassName(string className) {
            this.singletonClassName = className;
            return this;
        }

        public SingletonCodeGenerator SetSingletonInstanceName(string instanceName) {
            this.singletonInstanceName = instanceName;
            return this;
        }

        public SingletonCodeGenerator SetSingletonFieldSuffix(string fieldSuffix) {
            this.singletonFieldSuffix = fieldSuffix;
            return this;
        }

        public SingletonCodeGenerator SetSingletonConceptName(string conceptName) {
            this.singletonConceptName = conceptName;
            return this;
        }

        public SingletonCodeGenerator SetSingletonDirectoryName(string directoryName) {
            this.singletonDirectoryName = directoryName;
            return this;
        }

        public string SingletonNameFilter(string name, bool isFirstLetterLowerCase) {
            singletonNameBuilder.Clear();
            singletonNameBuilder
                .Append(new string(name
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray()));
            singletonNameBuilder[0] = isFirstLetterLowerCase
                ? char.ToLower(singletonNameBuilder[0])
                : char.ToUpper(singletonNameBuilder[0]);
            name = singletonNameBuilder.ToString();
            var suffix = singletonFieldSuffix ?? singletonConceptName ?? "";
            var suffixIndex = name.LastIndexOf(suffix);
            return suffixIndex == -1
                ? name
                : name.Substring(0, suffixIndex);
        }

        public SingletonCodeGenerator AddSingletonHeader() {
            singletonCodeBuilder
                .AppendLine("////////////////////////////////////////////////////////////////////")
                .AppendLine("/////////////////// AUTOMATICALLY GENERATED FILE ///////////////////")
                .AppendLine("////////////////////////////////////////////////////////////////////")
                .AppendLine()
                .AppendLine("using UnityEngine;")
                .AppendLine()
                .AppendLine($"namespace {singletonNamespaceName} {{")
                .AppendLine($"\tpublic{singletonClassModifiers} class {singletonClassName} {{")
                .AppendLine($"\t\tpublic static {singletonClassName} _instance;")
                .AppendLine($"\t\tpublic static {singletonClassName} {singletonInstanceName} {{")
                .AppendLine("\t\t\tget {")
                .AppendLine("\t\t\t\tif (_instance == null) {")
                .AppendLine($"\t\t\t\t\t_instance = new {singletonClassName}();")
                .AppendLine($"\t\t\t\t\t_instance.Assign{singletonConceptName}References();")
                .AppendLine("\t\t\t\t}")
                .AppendLine("\t\t\t\treturn _instance;")
                .AppendLine("\t\t\t}")
                .AppendLine("\t\t}")
                .AppendLine()
                .AppendLine($"\t\tpublic void Assign{singletonConceptName}References() {{");
            return this;
        }

        public SingletonCodeGenerator AddSingletonCustomCode(Action<SingletonCodeGenerator> codeGenerator) {
            codeGenerator?.Invoke(this);
            return this;
        }

        public SingletonCodeGenerator AddSingletonFooter() {
            singletonCodeBuilder
                .AppendLine("\t}")
                .AppendLine("}");
            return this;
        }

        public SingletonCodeGenerator GenerateSingletonAssets() {
            var codeDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, singletonDirectoryName));
            TryCreateDirectoryAsset(codeDirectory);
            var code = singletonCodeBuilder.ToString();
            singletonCodePath = Path.Combine(codeDirectory, $"{singletonClassName}.cs");
            TryCreateFileAsset(code, singletonCodePath);
            return this;
        }

        public SingletonCodeGenerator StartSingletonCodegenTimer() {
            singletonCodegenTimer.Start();
            return this;
        }
        public SingletonCodeGenerator EndSingletonCodegenTimerAndPrint() {
            singletonCodegenTimer.Stop();
            if (singletonCodePath != null) {
                Debug.Log($"Generated {singletonClassName}.cs in {singletonCodegenTimer.ElapsedMilliseconds} ms at {singletonCodePath}");
            } else {
                Debug.LogError($"Failed to generate {singletonClassName}.cs");
            }
            return this;
        }
    }
}
