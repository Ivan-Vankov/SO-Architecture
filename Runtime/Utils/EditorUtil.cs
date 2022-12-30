using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Vaflov {
    public static class EditorUtil {
        #if UNITY_EDITOR
        public static bool TryDeleteAsset(UnityEngine.Object asset) {
            string path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
                return false;
            if (!EditorUtility.DisplayDialog("Delete selected asset?",
                path + Environment.NewLine + "You cannot undo this action.", "Delete", "Cancel"))
                return false;
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
            return true;
        }
        #endif
    }
}
