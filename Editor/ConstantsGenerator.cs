﻿using System;
using System.Text;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Microsoft.CSharp;
using System.CodeDom;
using static Vaflov.TypeUtil;
using static Vaflov.FileUtil;
using static Vaflov.SingletonCodeGenerator;
using Sirenix.OdinInspector.Editor;
using UnityEditor.Callbacks;
using System.IO;

namespace Vaflov {
    public class ConstantsGenerator {
        public static readonly string GENERATED_CONSTANT_NAME_KEY = nameof(GENERATED_CONSTANT_NAME_KEY);
        public static readonly string GENERATED_CONSTANT_TYPE_KEY = nameof(GENERATED_CONSTANT_TYPE_KEY);

        public static UnityEngine.Object GenerateConstantAsset(string name, Type wrappedConstantType) {
            var constantType = TypeCache.GetTypesDerivedFrom(typeof(Constant<>))
                .Where(type => {
                    if (type.IsGenericType)
                        return false;
                    return type.BaseType.GenericTypeArguments[0] == wrappedConstantType;
                    //type.GenericArgumentsContainsTypes(wrappedConstantType);
                    //type.GenericParameterIsFulfilledBy
                })
                .FirstOrDefault();

            //Debug.Log(constantType?.Name);

            if (constantType == null) {
                EditorPrefs.SetString(GENERATED_CONSTANT_NAME_KEY, name);
                EditorPrefs.SetString(GENERATED_CONSTANT_TYPE_KEY, wrappedConstantType.AssemblyQualifiedName);
                GenerateConstantClass(wrappedConstantType);
                return null;
            }

            var constantAsset = ScriptableObject.CreateInstance(constantType);
            AssetDatabase.CreateAsset(constantAsset, $"Assets/Resources/Constant/{name}.asset");
            AssetDatabase.SaveAssets();
            return constantAsset;
        }

        public static void GenerateConstantClass(Type wrappedConstantType) {
            var wrappedClassName = wrappedConstantType.Name.Replace("+", "");
            var wrapperClassName = wrappedClassName + "Constant";
            var wrappedDerivedClassName = wrappedConstantType.FullName.Replace('+', '.');
            var code =
                "namespace Vaflov {" +
                $"\n\tpublic class {wrapperClassName} : Constant<{wrappedDerivedClassName}> {{ }}" +
                "\n}" +
                "\n";
            var codeDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, Config.PACKAGE_NAME, "Generated", "Constants"));
            if (!Directory.Exists(codeDirectory)) {
                Directory.CreateDirectory(codeDirectory);
            }

            var codePath = Path.Combine(codeDirectory, $"{wrapperClassName}.cs");
            TryCreateFileAsset(code, codePath);
        }

        [DidReloadScripts]
        private static void TryGenerateConstantAssetDelayed() {
            if (!EditorPrefs.HasKey(GENERATED_CONSTANT_NAME_KEY)
             || !EditorPrefs.HasKey(GENERATED_CONSTANT_TYPE_KEY)) {
                //Debug.Log("early out");
                return;
            }
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) {
                //Debug.Log("Delayed");
                UnityEditorEventUtility.DelayAction(TryGenerateConstantAssetDelayed);
                return;
            }
            //Debug.Log("Generating asset");
            var constantName = EditorPrefs.GetString(GENERATED_CONSTANT_NAME_KEY);
            var wrappedConstantType = Type.GetType(EditorPrefs.GetString(GENERATED_CONSTANT_TYPE_KEY));
            //Debug.Log($"{EditorPrefs.GetString(GENERATED_CONSTANT_TYPE_KEY)}, {wrappedConstantType}");

            EditorPrefs.DeleteKey(GENERATED_CONSTANT_NAME_KEY);
            EditorPrefs.DeleteKey(GENERATED_CONSTANT_TYPE_KEY);
            GenerateConstantAsset(constantName, wrappedConstantType);
        }

        [MenuItem("Tools/SO Architecture/Generate Constants")]
        public static void GenerateConstants() {
            new SingletonCodeGenerator(singletonClassName: "Constants", singletonConceptName: "Constant")
            .StartSingletonCodegenTimer()
            .AddSingletonHeader()
            .AddSingletonCustomCode(codegen => {
                (var namespaceName, var className        ) = (codegen.singletonNamespaceName, codegen.singletonClassName  );
                (var instanceName , var conceptName      ) = (codegen.singletonInstanceName , codegen.singletonConceptName);
                (var codeBuilder  , NameFilter nameFilter) = (codegen.singletonCodeBuilder  , codegen.SingletonNameFilter );

                var namespacePrefix = $"{namespaceName}.";
                using var codeProvider = new CSharpCodeProvider();
                string GetTruncatedTypeName(Type type) {
                    var realTypeName = codeProvider.GetTypeOutput(new CodeTypeReference(type));
                    return realTypeName.StartsWith(namespacePrefix)
                        ? realTypeName.Substring(namespacePrefix.Length)
                        : realTypeName;
                }

                var constantsCodeBuilder = new StringBuilder();
                var constantTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsClass && !type.IsGenericType && !type.IsAbstract && IsInheritedFrom(type, typeof(Constant<>)))
                    .ToList();
                foreach (var constantType in constantTypes) {
                    var constantAssetGuids = AssetDatabase.FindAssets($"t: {constantType}");
                    foreach (var constantAssetGuid in constantAssetGuids) {
                        var assetPath = AssetDatabase.GUIDToAssetPath(constantAssetGuid);
                        var constantAsset = AssetDatabase.LoadAssetAtPath(assetPath, constantType);

                        var constantFieldTypeName = constantType.Name;
                        var constantFieldGenericType = constantType.BaseType.GenericTypeArguments[0];
                        var constantFieldGenericTypeName = GetTruncatedTypeName(constantFieldGenericType);
                        var constantFieldName = nameFilter(constantAsset.name, true);

                        var resourcesPathName = "Resources";
                        var resourcesIdx = assetPath.IndexOf(resourcesPathName);
                        if (resourcesIdx == -1) {
                            Debug.LogError($"{conceptName} {constantFieldName} at path {assetPath} is not in a {resourcesPathName} folder or subfolder");
                            continue;
                        }
                        var resourcesAssetPath = assetPath.Substring(resourcesIdx + resourcesPathName.Length + 1);
                        resourcesAssetPath = resourcesAssetPath.Substring(0, resourcesAssetPath.LastIndexOf(".asset"));
                        codeBuilder
                            .AppendLine($"\t\t\t{constantFieldName} = {resourcesPathName}.Load<{constantFieldTypeName}>(\"{resourcesAssetPath}\");");

                        constantsCodeBuilder
                            .AppendLine($"\t\tpublic {constantFieldTypeName} @{constantFieldName};")
                            .AppendLine($"\t\tpublic static {constantFieldGenericTypeName} {nameFilter(constantAsset.name, false)} => {instanceName}.{constantFieldName}.Value;")
                            .AppendLine();
                    }
                }

                codeBuilder
                    .AppendLine("\t\t}")
                    .AppendLine()
                    .Append(constantsCodeBuilder.ToString());
            })
            .AddSingletonFooter()
            .GenerateSingletonAssets()
            .EndSingletonCodegenTimerAndPrint();
        }
    }

    public class ConstantsUpdater : AssetPostprocessor {
        public static bool CheckUpdateConstants(string[] modifiedAssetPaths) {
            for (int i = 0; i < modifiedAssetPaths.Length; ++i) {
                if (modifiedAssetPaths[i].EndsWith("Constant.asset")) {
                    ConstantsGenerator.GenerateConstants();
                    return true;
                }
            }
            return false;
        }

        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            if (CheckUpdateConstants(importedAssets)) { return; }
            if (CheckUpdateConstants(deletedAssets)) { return; }
            if (CheckUpdateConstants(movedAssets)) { return; }
            if (CheckUpdateConstants(movedFromAssetPaths)) { return; }
        }
    }
}