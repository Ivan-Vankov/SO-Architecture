#if ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Vaflov {
    public class RuntimeSetEditorWindow : EditorObjectMenuEditorWindow {
        public override Type EditorObjBaseType => typeof(RuntimeSet<>);

        [MenuItem("Tools/SO Architecture/Runtime Set Editor")]
        public static RuntimeSetEditorWindow Open() {
            return Open<RuntimeSetEditorWindow>("Runtime Sets", "set");
        }

        public override IEditorObjectCreator CreateEditorObjectCreator() =>
            new DefaultEditorObjectCreator(EditorObjBaseType, "New Runtime Set", "Add a new runtime set", RuntimeSetGenerator.GenerateAsset)
                .SetTypeFilter(type => TypeUtil.IsInheritedFrom(type, typeof(UnityEngine.Object)), typeof(GameObject));
    }
}
#endif