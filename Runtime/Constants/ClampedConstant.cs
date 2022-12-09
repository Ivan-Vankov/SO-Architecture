#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System;

namespace Vaflov {
    #if ODIN_INSPECTOR
    public static class ClampedConstantEditorEvents {
        public static Action OnConstantClampedChanged;
    }
    #endif

    public class ClampedConstant<T> : Constant<T> where T: IComparable, IComparable<T>, IEquatable<T> {
        #if ODIN_INSPECTOR
        [LabelWidth(60)]
        [PropertyOrder(100)]
        [OnValueChanged(nameof(OnClampedChanged))]
        #endif
        public bool clamped = false;

        #if ODIN_INSPECTOR
        [MaxValue(nameof(max))]
        [ShowIf(nameof(clamped))]
        [LabelWidth(30)]
        [HorizontalGroup("Slider")]
        [PropertyOrder(15)]
#endif
        public T min;

        #if ODIN_INSPECTOR
        [MinValue(nameof(min))]
        [ShowIf(nameof(clamped))]
        [LabelWidth(30)]
        [HorizontalGroup("Slider")]
        [PropertyOrder(16)]
#endif
        public T max;

        #if ODIN_INSPECTOR
        public void OnClampedChanged() {
            ClampedConstantEditorEvents.OnConstantClampedChanged.Invoke();
        }
        #endif
    }
}