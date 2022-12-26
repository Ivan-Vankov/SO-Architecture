using Sirenix.OdinInspector;
using System;
using static Vaflov.Config;

namespace Vaflov {
#if ODIN_INSPECTOR
    public static class RangeConstantEditorEvents {
        public static Action OnConstantRangeChanged;
    }
#endif

    public class RangeConstant<T> : Constant<T> {
        [LabelText("Range")]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(13)]
        [OnValueChanged(nameof(OnRangeChanged))]
        public bool isRange = false;

        [HideLabel]
        [ShowIf(nameof(isRange))]
        [PropertyOrder(16)]
        public T range;


#if ODIN_INSPECTOR
        public void OnRangeChanged() {
            RangeConstantEditorEvents.OnConstantRangeChanged.Invoke();
        }
#endif
    }
}
