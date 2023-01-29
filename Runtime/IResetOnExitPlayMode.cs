using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Vaflov {
    public interface IResetOnExitPlayMode { }

    #if UNITY_EDITOR
    public static class SOPlayModeResetter {
        public static Dictionary<ScriptableObject, string> defaultSOValues = new Dictionary<ScriptableObject, string>();

        [InitializeOnLoadMethod]
        public static void RegisterResets() {
            EditorApplication.playModeStateChanged += ResetSOsWithIResetOnExitPlayMode;
        }

        public static void ResetSOsWithIResetOnExitPlayMode(PlayModeStateChange change) {
            if (change == PlayModeStateChange.EnteredPlayMode) {
                foreach (var asset in FindAssets<ScriptableObject>()) {
                    if (asset is IResetOnExitPlayMode) {
                        defaultSOValues[asset] = JsonUtility.ToJson(asset);
                    }
                }
            } else if (change == PlayModeStateChange.ExitingPlayMode) {
                foreach (var asset in FindAssets<ScriptableObject>()) {
                    if (asset is IResetOnExitPlayMode) {
                        var defaultValue = defaultSOValues[asset];
                        if (defaultValue != null) {
                            //EditorJsonUtility.FromJsonOverwrite(defaultValue, asset);
                            JsonUtility.FromJsonOverwrite(defaultValue, asset);
                        }
                    }
                }
                defaultSOValues.Clear();
                AssetDatabase.SaveAssets();
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
