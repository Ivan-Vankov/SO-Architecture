using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Vaflov {
    public static class AssetDatabaseUtil {
        public static T SaveScriptableObject<T>(string dir, string name) where T : ScriptableObject {
            return (T)SaveScriptableObject(typeof(T), dir, name);
        }

        public static ScriptableObject SaveScriptableObject(Type soType, string dir, string name) {
            #if UNITY_EDITOR
            var soAsset = ScriptableObject.CreateInstance(soType);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            var path = $"{dir}/{name}.asset";
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CreateAsset(soAsset, path);
            AssetDatabase.SaveAssets();
            return soAsset;
            #else
            return null;
            #endif
        }
    }
}
