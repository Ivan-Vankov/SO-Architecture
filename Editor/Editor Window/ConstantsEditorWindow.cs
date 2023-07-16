#if ODIN_INSPECTOR && UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;

namespace Vaflov {
    public class ConstantsEditorWindow : EditorObjectMenuEditorWindow {
        public override Type EditorObjBaseType => typeof(Constant<>);

        public override string DefaultEditorObjFolderPath() => "Assets/Resources/Constant";

        public override IEditorObjectCreator CreateEditorObjectCreator() =>
            new GenericEditorObjectCreator(EditorObjBaseType, "New Constant", "Add", ConstantsGenerator.GenerateConstantAsset);

        [MenuItem("Tools/" + SOArchitectureConfig.PACKAGE_NAME + "/Constants Editor", priority = 10)]
        public static ConstantsEditorWindow Open() => Open<ConstantsEditorWindow>("Constants", "pi");

        [MenuItem("Assets/Create/" + SOArchitectureConfig.PACKAGE_NAME + "/Constant", priority = 10)]
        public static void CreateConstant() => Open().TryOpenEditorObjectCreationMenu();

        public override List<OdinContextMenuItem> GetToolbarItems() {
            var items = new List<OdinContextMenuItem>();
            items.AddRange(base.GetToolbarItems());
            items.Add(new OdinContextMenuItem("Regenerate constants", () => {
                ConstantsGenerator.GenerateConstants();
                //ForceMenuTreeRebuild();
            }, KeyCode.S, EventModifiers.Control, SdfIconType.ArrowRepeat));
            return items;
        }
    }
}
#endif