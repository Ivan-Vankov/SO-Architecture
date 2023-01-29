#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Vaflov {
    public static class AssetUtil {
        public static List<string> GetAssetPathsForType(Type baseType) {
            var types = TypeUtil.GetFlatTypesDerivedFrom(baseType);
            var assetNames = new List<string>();
            if (types == null) {
                return assetNames;
            }
            foreach (var type in types) {
                var assetGuids = AssetDatabase.FindAssets($"t: {type}");
                foreach (var assetGuid in assetGuids) {
                    var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                    var asset = AssetDatabase.LoadAssetAtPath(assetPath, type);
                    assetNames.Add(asset.name);
                }
            }
            return assetNames;
        }
    }
}
#endif