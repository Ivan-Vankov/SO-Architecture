using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vaflov {
    public static partial class EditorStringUtil {
        public static string ValidateAssetName(string targetName, List<string> assetNames) {
            for (int i = 0; i < assetNames.Count; ++i) {
                if (string.Compare(assetNames[i], targetName, StringComparison.OrdinalIgnoreCase) == 0) {
                    return "Name is not unique";
                }
            }
            targetName = targetName.RemoveWhitespaces();
            return ValidateArgName(targetName);
        }

        public static string ValidateArgName(string argName) {
            if (argName.Length == 0)
                return "Name is empty";
            if (argName[0] != '_' && !char.IsLetter(argName[0]))
                return "The first character should be _ or a letter";
            for (int i = 1; i < argName.Length; ++i) {
                var c = argName[i];
                if (!char.IsLetter(c) && !char.IsDigit(c) && c != '_')
                    return "Name contains a character that is not \'_\', a letter or a digit";
            }
            return null;
        }
    }
}
