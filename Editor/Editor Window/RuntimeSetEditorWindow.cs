using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Vaflov {
    public class RuntimeSetEditorWindow : EditorObjectMenuEditorWindow {
        public static readonly Vector2Int DEFAULT_EDITOR_SIZE = new Vector2Int(600, 400);
        public override Type EditorObjBaseType => typeof(RuntimeSet<>);

        [MenuItem("Tools/SO Architecture/Runtime Set Editor")]
        public static RuntimeSetEditorWindow Open() {
            return Open<RuntimeSetEditorWindow>("Runtime Sets", DEFAULT_EDITOR_SIZE);
        }
    }
}
