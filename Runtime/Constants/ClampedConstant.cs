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
        [PropertyOrder(100)]        
        [OnValueChanged(nameof(OnClampedChanged))]
        #endif
        public bool isClamped = false;

        #if ODIN_INSPECTOR
        [MaxValue(nameof(max))]
        [ShowIf(nameof(isClamped))]
        [PropertyOrder(15)]
        [LabelWidth(30)]
        [HorizontalGroup("Slider")]
        #endif
        public T min;

        #if ODIN_INSPECTOR
        [MinValue(nameof(min))]
        [ShowIf(nameof(isClamped))]
        [PropertyOrder(16)]
        [LabelWidth(30)]
        [HorizontalGroup("Slider")]
        #endif
        public T max;

        #if ODIN_INSPECTOR
        public void OnClampedChanged() {
            ClampedConstantEditorEvents.OnConstantClampedChanged.Invoke();
        }
        #endif
    }
}