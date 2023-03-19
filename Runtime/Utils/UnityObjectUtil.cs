#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Vaflov {
    public class UnityObjectUtil : MonoBehaviour {
        public static bool IsSavedAsAsset(Object obj) {
            #if UNITY_EDITOR
            if (!obj)
                return false;
            string assetPath = AssetDatabase.GetAssetPath(obj);
            return !string.IsNullOrEmpty(assetPath);
            #else
            return false;
            #endif
        }
    }
}
