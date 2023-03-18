#if ODIN_INSPECTOR && UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Vaflov;

[assembly: RegisterStateUpdater(typeof(IRuntimeAssetStateUpdater))]

namespace Vaflov {
    public class IRuntimeAssetStateUpdater : ValueStateUpdater<IRuntimeAsset> {
        public override bool CanUpdateProperty(InspectorProperty property) {
            return property.IsTreeRoot;
        }

        public override void OnStateUpdate() {
            if (Event.current != null && InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth == 0) {
                GUI.enabled = false;
                EditorGUILayout.LabelField(new GUIContent("Runtime asset", "The asset will be reset on play mode exit"));
                GUI.enabled = true;
            }
        }
    }
}
#endif