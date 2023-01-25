#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;

namespace Vaflov {
    [CustomEditor(typeof(RuntimeScriptableObject), true)]
    public class RuntimeScriptableObjectEditor : OdinEditor {
        public override void OnInspectorGUI() {
            if (EditorApplication.isPlaying) {
                SirenixEditorGUI.InfoMessageBox("Scriptable object will be reset on play mode exit");
            }
            base.OnInspectorGUI();
        }
    }
}
#endif