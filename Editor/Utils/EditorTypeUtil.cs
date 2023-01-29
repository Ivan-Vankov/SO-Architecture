#if ODIN_INSPECTOR && UNITY_EDITOR
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Vaflov {
    public static partial class EditorTypeUtil {
        public static List<Type> GatherPublicTypes() {
            return AssemblyUtilities.GetTypes(AssemblyTypeFlags.GameTypes | AssemblyTypeFlags.PluginEditorTypes)
            .Where(x => {
                if (x.Name == null || x.IsGenericType || x.IsNotPublic)
                    return false;
                string text = x.Name.TrimStart(Array.Empty<char>());
                return text.Length != 0 && char.IsLetter(text[0]);
            }).ToList();
        }
    }
}
#endif