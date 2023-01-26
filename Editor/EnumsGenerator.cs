using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Vaflov.TypeUtil;
using static Vaflov.SingletonCodeGenerator;
using Sirenix.Utilities;

namespace Vaflov {
    public class EnumsGenerator {
        //[MenuItem("Tools/SO Architecture/Generate Enums")]
        public static void GenerateEnums() {
            TypeCache.GetTypesDerivedFrom<ScriptableEnum>().ForEach(GenerateEnum);
            //AppDomain.CurrentDomain.GetAssemblies()
            //    .SelectMany(assembly => assembly.GetTypes())
            //    .Where(type => type.IsClass && !type.IsAbstract && IsInheritedFrom(type, typeof(ScriptableEnum)))
            //    .ToList()
            //    .ForEach(GenerateEnum);
        }

        public static void GenerateEnum(Type enumType) {
            var enumName = enumType.Name;
            new SingletonCodeGenerator(singletonClassName: enumName, singletonConceptName: enumName)
            .SetSingletonInstancerString($"CreateInstance<{enumName}>()")
            .StartSingletonCodegenTimer()
            .SetSingletonClassModifiers(" partial")
            .AddSingletonHeader()
            .AddSingletonCustomCode(codegen => {
                (var namespaceName, var className) = (codegen.singletonNamespaceName, codegen.singletonClassName);
                (var instanceName, var conceptName) = (codegen.singletonInstanceName, codegen.singletonConceptName);
                (var codeBuilder, NameFilter nameFilter) = (codegen.singletonCodeBuilder, codegen.SingletonNameFilter);

                var namespacePrefix = $"{namespaceName}.";

                var enumCodeBuilder = new StringBuilder();
                var enumAssetGuids = AssetDatabase.FindAssets($"t: {enumType}");
                foreach (var enumAssetGuid in enumAssetGuids) {
                    var assetPath = AssetDatabase.GUIDToAssetPath(enumAssetGuid);
                    var enumAsset = AssetDatabase.LoadAssetAtPath(assetPath, enumType);

                    var enumFieldTypeName = enumType.Name;
                    var enumFieldName = "_" + nameFilter(enumAsset.name, true);

                    var resourcesPathName = "Resources";
                    var resourcesIdx = assetPath.IndexOf(resourcesPathName);
                    if (resourcesIdx == -1) {
                        Debug.LogError($"{conceptName} {enumFieldName} at path {assetPath} is not in a {resourcesPathName} folder or subfolder");
                        continue;
                    }
                    var resourcesAssetPath = assetPath.Substring(resourcesIdx + resourcesPathName.Length + 1);
                    resourcesAssetPath = resourcesAssetPath.Substring(0, resourcesAssetPath.LastIndexOf(".asset"));
                    codeBuilder
                        .AppendLine($"\t\t\t{enumFieldName} = {resourcesPathName}.Load<{enumFieldTypeName}>(\"{resourcesAssetPath}\");");

                    enumCodeBuilder
                        .AppendLine($"\t\tpublic {enumFieldTypeName} {enumFieldName};")
                        .AppendLine($"\t\tpublic static {enumFieldTypeName} {nameFilter(enumAsset.name, false)} => {instanceName}.{enumFieldName};")
                        .AppendLine();
                }

                codeBuilder
                    .AppendLine("\t\t}")
                    .AppendLine()
                    .Append(enumCodeBuilder.ToString());
            })
            .AddSingletonFooter()
            .GenerateSingletonAssets()
            .EndSingletonCodegenTimerAndPrint();
        }
    }

    public class EnumsUpdater : AssetPostprocessor {
        public struct TypeWithSuffix {
            public Type type;
            public string suffix;

            public TypeWithSuffix(Type type, string suffix) {
                this.type = type;
                this.suffix = suffix;
            }

            public void Deconstruct(out Type type, out string suffix) {
                type = this.type;
                suffix = this.suffix;
            }
        }

        public static bool CheckUpdateEnums(string[] modifiedAssetPaths, HashSet<TypeWithSuffix> typesWithSuffixes) {
            var excludedTypes = new HashSet<Type>();
            for (int i = 0; i < modifiedAssetPaths.Length; ++i) {
                if (typesWithSuffixes.Count == 0) {
                    break;
                }
                excludedTypes.Clear();
                foreach ((var type, var suffix) in typesWithSuffixes) {
                    if (modifiedAssetPaths[i].EndsWith(suffix)) {
                        EnumsGenerator.GenerateEnum(type);
                        excludedTypes.Add(type);
                    }
                }
                typesWithSuffixes.RemoveWhere(typeWithSuffix => excludedTypes.Contains(typeWithSuffix.type));
            }
            return typesWithSuffixes.Count == 0;
        }

        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            var enumTypesWithSuffixes = new HashSet<TypeWithSuffix>(AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsClass && !type.IsAbstract && IsInheritedFrom(type, typeof(ScriptableEnum)))
                .Select(type => new TypeWithSuffix(type, (type.GetCustomAttributes(typeof(CreateAssetMenuAttribute), false)
                    ?.FirstOrDefault() as CreateAssetMenuAttribute)
                    ?.fileName
                    ?.Append(".asset")))
                .Where(typeWithSuffix => typeWithSuffix.suffix != null));

            if (CheckUpdateEnums(importedAssets, enumTypesWithSuffixes)) { return; }
            if (CheckUpdateEnums(deletedAssets, enumTypesWithSuffixes)) { return; }
            if (CheckUpdateEnums(movedAssets, enumTypesWithSuffixes)) { return; }
            if (CheckUpdateEnums(movedFromAssetPaths, enumTypesWithSuffixes)) { return; }
        }
    }
}