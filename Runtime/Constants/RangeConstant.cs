#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System;
using static Vaflov.SOArchitectureConfig;

namespace Vaflov {
    public static class RangeConstant {
        public static Action OnConstantRangeChanged;
    }

    [CodegenInapplicable]
    public class RangeConstant<T, C> : Constant<T> {
        #if ODIN_INSPECTOR
        [LabelText("Range")]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(13)]
        [OnValueChanged(nameof(OnRangeChanged))]
        #endif
        public bool isRange = false;

        #if ODIN_INSPECTOR
        [MaxValue(nameof(max))]
        [ShowIf(nameof(isRange))]
        [LabelWidth(30)]
        [HorizontalGroup("Slider")]
        [PropertyOrder(15)]
        #endif
        public C min;

        #if ODIN_INSPECTOR
        [MinValue(nameof(min))]
        [ShowIf(nameof(isRange))]
        [LabelWidth(30)]
        [HorizontalGroup("Slider")]
        [PropertyOrder(16)]
        #endif
        public C max;

        public void OnRangeChanged() {
            RangeConstant.OnConstantRangeChanged.Invoke();
        }
    }
}
