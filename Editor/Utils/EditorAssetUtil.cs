#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Vaflov {
    public static class EditorAssetUtil {
        public static List<string> GetAssetPathsForType(Type baseType, string[] folders = null) {
            var assetNames = new List<string>();
            var assetGuids = AssetDatabase.FindAssets($"t: {baseType}", folders);
            foreach (var assetGuid in assetGuids) {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                var asset = AssetDatabase.LoadAssetAtPath(assetPath, baseType);
                if (asset) {
                    assetNames.Add(asset.name);
                }
            }
            return assetNames;
        }
    }
}
#endif