#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
#endif
using UnityEngine;
using static Vaflov.Config;

namespace Vaflov {
    public class QuaternionConstant : Constant<Quaternion> {
        #if ODIN_INSPECTOR
        [ShowInInspector]
        [EnumToggleButtons]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(12)]
        public QuaternionDrawMode DrawMode {
            get => GlobalConfig<GeneralDrawerConfig>.Instance.QuaternionDrawMode;
            set => GlobalConfig<GeneralDrawerConfig>.Instance.QuaternionDrawMode = value;
        }
        #endif
    }
}