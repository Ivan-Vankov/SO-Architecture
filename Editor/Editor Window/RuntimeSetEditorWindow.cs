#if ODIN_INSPECTOR
using System;
using UnityEditor;
using UnityEngine;

namespace Vaflov {
    public class RuntimeSetEditorWindow : EditorObjectMenuEditorWindow {
        public override Type EditorObjBaseType => typeof(RuntimeSet<>);

        public override IEditorObjectCreator CreateEditorObjectCreator() =>
            new GenericEditorObjectCreator(EditorObjBaseType, "New Runtime Set", "Add a new runtime set", RuntimeSetGenerator.GenerateAsset)
                .SetTypeFilter(type => TypeUtil.IsInheritedFrom(type, typeof(UnityEngine.Object)), typeof(GameObject));

        [MenuItem("Tools/" + Config.PACKAGE_NAME + "/Runtime Set Editor", priority = 30)]
        public static RuntimeSetEditorWindow Open() => Open<RuntimeSetEditorWindow>("Runtime Sets", "set");

        [MenuItem("Assets/Create/" + Config.PACKAGE_NAME + "/Runtime Set", priority = 30)]
        public static void CreateRuntimeSet() => Open().TryOpenEditorObjectCreationMenu();
    }
}
#endif