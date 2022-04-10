using System.IO;
using UnityEditor;
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

        public static void TryCreateDirectoryAsset(string dirPath) {
            if (Directory.Exists(dirPath)) { return; }
            Directory.CreateDirectory(dirPath);
            var dirAssetPath = FullPathToAssetPath(dirPath);
            if (dirAssetPath == null) { return; }
            AssetDatabase.ImportAsset(dirAssetPath, ImportAssetOptions.ForceUpdate);
        }

        public static void TryCreateFileAsset(string fileContents, string filePath) {            
            using (var fileStream = new StreamWriter(filePath, append: false)) {
                fileStream.Write(fileContents);
                fileStream.Flush();
            }

            var fileAssetPath = FullPathToAssetPath(filePath);
            if (fileAssetPath == null) { return; }
            AssetDatabase.ImportAsset(fileAssetPath, ImportAssetOptions.ForceUpdate);
        }
    }
}
