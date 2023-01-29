#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System;
using static Vaflov.Config;

namespace Vaflov {
    public static class ClampedConstant {
        public static Action OnConstantClampedChanged;
    }

    public class ClampedConstant<T> : Constant<T> where T: IComparable, IComparable<T>, IEquatable<T> {
        #if ODIN_INSPECTOR
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(13)]
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

        public void OnClampedChanged() {
            ClampedConstant.OnConstantClampedChanged?.Invoke();
        }
    }
}