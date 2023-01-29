using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Vaflov {
    public static class FileUtil {
        public static string FullPathToAssetPath(string fullPath) {
            var pathIndex = fullPath != null
                ? fullPath.LastIndexOf($"Assets")
                : -1;
            if (pathIndex == -1) {
                pathIndex = fullPath.LastIndexOf($"Packages");
            }
            return pathIndex == -1
                ? null
                : fullPath
                    .Substring(pathIndex)
                    .Replace('\\', '/'); // A bit sus but should work for now
        }

        #if UNITY_EDITOR
        public static string TryCreateFileAsset(string fileContents, string fileName, 
                                                ImportAssetOptions importAssetOptions = ImportAssetOptions.ForceUpdate,
                                                Texture2D icon = null,
                                                params string[] directoryArgs) {
            var fileDirectory = Path.GetFullPath(Path.Combine(directoryArgs));
            if (!Directory.Exists(fileDirectory)) {
                Directory.CreateDirectory(fileDirectory);
            }

            var filePath = Path.Combine(fileDirectory, fileName);

            using (var fileStream = new StreamWriter(filePath, append: false)) {
                fileStream.Write(fileContents);
                fileStream.Flush();
            }

            var fileAssetPath = FullPathToAssetPath(filePath);
            if (fileAssetPath == null) { 
                return null; 
            }
            AssetDatabase.ImportAsset(fileAssetPath, importAssetOptions);
            if (icon != null) {
                if (AssetDatabase.GetMainAssetTypeAtPath(fileAssetPath) == typeof(MonoScript)) {
                    var monoImporter = AssetImporter.GetAtPath(fileAssetPath) as MonoImporter;
                    monoImporter.SetIcon(icon);
                    monoImporter.SaveAndReimport();
                }
            }
            return fileAssetPath;
        }
        #endif
    }
}
