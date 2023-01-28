#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Vaflov;

[assembly: RegisterStateUpdater(typeof(IResetOnExitPlayModeStateUpdater))]

namespace Vaflov {
    public class IResetOnExitPlayModeStateUpdater : ValueStateUpdater<IResetOnExitPlayMode> {
        public override bool CanUpdateProperty(InspectorProperty property) {
            return property.IsTreeRoot;
        }

        public override void OnStateUpdate() {
            SirenixEditorGUI.InfoMessageBox("Object will be reset on play mode exit");
        }
    }
}
#endif