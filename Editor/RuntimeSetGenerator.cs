using Sirenix.OdinInspector.Editor;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using Sirenix.Utilities;
using static Vaflov.FileUtil;

namespace Vaflov {
    public static class RuntimeSetGenerator {
        public static readonly string GENERATED_RUNTIME_SET_NAME_KEY = nameof(GENERATED_RUNTIME_SET_NAME_KEY);
        public static readonly string GENERATED_RUNTIME_SET_TYPE_KEY = nameof(GENERATED_RUNTIME_SET_TYPE_KEY);

        public static void GenerateAsset(string name, Type wrappedType) {
            GenerateAsset(name, wrappedType, true);
        }

        public static void GenerateAsset(string name, Type wrappedType, bool generateClass) {
            var targetType = TypeCache.GetTypesDerivedFrom(typeof(RuntimeSet<>))
                .Where(type => {
                    if (type.IsGenericType)
                        return false;
                    return type.BaseType.GenericTypeArguments[0] == wrappedType;
                })
                .FirstOrDefault();

            if (targetType == null) {
                if (generateClass) {
                    EditorPrefs.SetString(GENERATED_RUNTIME_SET_NAME_KEY, name);
                    EditorPrefs.SetString(GENERATED_RUNTIME_SET_TYPE_KEY, wrappedType.AssemblyQualifiedName);
                    GenerateClass(wrappedType);
                }
                return;
            }

            AssetDatabaseUtil.SaveScriptableObject(targetType, "Assets/Resources/Runtime Sets", name);
        }

        public static void GenerateClass(Type wrappedType) {
            var wrappedClassName = wrappedType.Name.Replace("+", "");
            var wrapperClassName = wrappedClassName + "Set";
            var wrappedDerivedClassName = wrappedType.FullName.Replace('+', '.');

            var code =
                "namespace Vaflov {" +
                $"\n\tpublic class {wrapperClassName} : RuntimeSet<{wrappedDerivedClassName}> {{ }}" +
                "\n}" +
                "\n";

            TryCreateFileAsset(code, $"{wrapperClassName}.cs",
                ImportAssetOptions.ForceUpdate,
                Resources.Load<Texture2D>("Set Large"),
                Application.dataPath, Config.PACKAGE_NAME, "Generated", "Runtime Sets");
        }

        [DidReloadScripts]
        private static void TryGenerateAssetDelayed() {
            if (!EditorPrefs.HasKey(GENERATED_RUNTIME_SET_NAME_KEY)
                || !EditorPrefs.HasKey(GENERATED_RUNTIME_SET_TYPE_KEY)) {
                //Debug.Log("early out");
                return;
            }
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) {
                //Debug.Log("Delayed");
                UnityEditorEventUtility.DelayAction(TryGenerateAssetDelayed);
                return;
            }
            //Debug.Log("Generating asset");
            var constantName = EditorPrefs.GetString(GENERATED_RUNTIME_SET_NAME_KEY);
            var wrappedConstantType = Type.GetType(EditorPrefs.GetString(GENERATED_RUNTIME_SET_TYPE_KEY));
            //Debug.Log($"{EditorPrefs.GetString(GENERATED_CONSTANT_TYPE_KEY)}, {wrappedConstantType}");

            EditorPrefs.DeleteKey(GENERATED_RUNTIME_SET_NAME_KEY);
            EditorPrefs.DeleteKey(GENERATED_RUNTIME_SET_TYPE_KEY);
            GenerateAsset(constantName, wrappedConstantType, false);
        }
    }
}
