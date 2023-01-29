#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.Utilities;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
#endif
#endif
using UnityEngine;
using static Vaflov.Config;

namespace Vaflov {
    public class QuaternionConstant : Constant<Quaternion> {
        #if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
        [EnumToggleButtons]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(12)]
        public QuaternionDrawMode DrawMode {
            get => GlobalConfig<GeneralDrawerConfig>.Instance.QuaternionDrawMode;
            set => GlobalConfig<GeneralDrawerConfig>.Instance.QuaternionDrawMode = value;
        }

        public override string EditorToString() {
            var v = Value;
            switch (DrawMode) {
                case QuaternionDrawMode.Eulers:
                    return v.eulerAngles.ToString();
                case QuaternionDrawMode.AngleAxis:
                    v.ToAngleAxis(out float angle, out Vector3 axis);
                    return axis.ToString() + " " + angle.ToString("0.00") + "°";
                case QuaternionDrawMode.Raw:
                    return new Vector4(v.x, v.y, v.z, v.w).ToString();
                default: return null;
            }
        }
        #endif
    }
}