#if ODIN_INSPECTOR
using Sirenix.Utilities;
#endif
using Microsoft.CSharp;
using System;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using Sirenix.Utilities.Editor;
using System.IO;
using System.Collections.Generic;

namespace Vaflov {
    public class SOArchitectureConfig
        #if ODIN_INSPECTOR
        : SerializedScriptableObject
        #else
        : ScriptableObject
        #endif
        {
        public const string PACKAGE_NAME = "SO Architecture";
        public const int preferedEditorLabelWidth = 70;
        public static readonly CSharpCodeProvider codeProvider = new CSharpCodeProvider();
        public static string AUTO_GENERATED_HEADER =
            "////////////////////////////////////////////////////////////////////" + Environment.NewLine
          + "/////////////////// AUTOMATICALLY GENERATED FILE ///////////////////" + Environment.NewLine
          + "////////////////////////////////////////////////////////////////////" + Environment.NewLine;

        [ReadOnly]
        public Dictionary<Type, List<string>> editorFolders;

        private static SOArchitectureConfig instance;
        public static SOArchitectureConfig Instance {
            get {
                if (instance == null) {
                    var dir = "Assets/Resources/Config";
                    var filePath = dir + "/SO Architecture Config.asset";
                    instance = AssetDatabase.LoadAssetAtPath<SOArchitectureConfig>(filePath);
                    if (instance == null) {
                        instance = CreateInstance<SOArchitectureConfig>();
                        #if ODIN_INSPECTOR && UNITY_EDITOR
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        filePath = AssetDatabase.GenerateUniqueAssetPath(filePath);
                        AssetDatabase.CreateAsset(instance, filePath);
                        AssetDatabase.SaveAssets();
                        #endif
                    }
                }
                return instance;
            }
        }
    }
}