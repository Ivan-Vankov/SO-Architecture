using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Vaflov {
    /// <summary>
    /// <see cref="ScriptableObject"/>s that implement <see cref="IRuntimeAsset"/> will have their
    /// values saved when play mode is entered and restored when play mode is exited.
    /// This is useful when you want to use <see cref="ScriptableObject"/>s as data containers.
    /// </summary>
    public interface IRuntimeAsset { }

    #if UNITY_EDITOR
    public static class SOPlayModeResetter {
        public static Dictionary<ScriptableObject, string> defaultSOValues = new Dictionary<ScriptableObject, string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void RegisterResets() {
            foreach (var asset in FindAssets<ScriptableObject>()) {
                if (asset is IRuntimeAsset) {
                    defaultSOValues[asset] = JsonUtility.ToJson(asset);
                }
            }

            EditorApplication.playModeStateChanged += ResetSOsWithIRuntimeAsset;
        }

        public static void ResetSOsWithIRuntimeAsset(PlayModeStateChange change) {
            if (change == PlayModeStateChange.EnteredEditMode) {
                foreach (var asset in FindAssets<ScriptableObject>()) {
                    if (asset is IRuntimeAsset) {
                        var defaultValue = defaultSOValues[asset];
                        if (defaultValue != null) {
                            //EditorJsonUtility.FromJsonOverwrite(defaultValue, asset);
                            JsonUtility.FromJsonOverwrite(defaultValue, asset);
                        }
                    }
                }
                defaultSOValues.Clear();
                AssetDatabase.SaveAssets();

                EditorApplication.playModeStateChanged -= ResetSOsWithIRuntimeAsset;
            }
        }

        public static IEnumerable<T> FindAssets<T>() where T : Object {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T)}");
            for (int i = 0; i < guids.Length; i++) {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                yield return AssetDatabase.LoadAssetAtPath<T>(path);
            }
        }
    }
    #endif
}
