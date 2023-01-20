#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using static UnityEngine.Mathf;
using System;
using Sirenix.OdinInspector;

namespace Vaflov {
    public class ConstantsEditorWindow : EditorObjectMenuEditorWindow {
        public override Type EditorObjBaseType => typeof(Constant<>);

        [MenuItem("Tools/SO Architecture/Constants Editor")]
        public static ConstantsEditorWindow Open() => Open<ConstantsEditorWindow>("Constants", "pi");

        public override IEditorObjectCreator CreateEditorObjectCreator() =>
            new DefaultEditorObjectCreator(EditorObjBaseType, "New Constant", "Add a new constant", ConstantsGenerator.GenerateConstantAsset);

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