#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System;
using static Vaflov.Config;

namespace Vaflov {
    #if ODIN_INSPECTOR
    public static class RangeConstantEditorEvents {
        public static Action OnConstantRangeChanged;
    }
    #endif

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

        #if ODIN_INSPECTOR
        public void OnRangeChanged() {
            RangeConstantEditorEvents.OnConstantRangeChanged.Invoke();
        }
        #endif
    }
}
