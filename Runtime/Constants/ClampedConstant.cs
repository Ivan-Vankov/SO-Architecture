//using Sirenix.OdinInspector;
//using Sirenix.OdinInspector.Editor;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System;

namespace Vaflov {
    public class ClampedConstant<T> : Constant<T> where T: IComparable, IComparable<T>, IEquatable<T> {
        public bool isClamped = false;
        #if ODIN_INSPECTOR
        [MaxValue("max")]
        #endif
        public T min;
        #if ODIN_INSPECTOR
        [MinValue("min")]
        #endif
        public T max;

        //[HideLabel]
        //[LabelWidth(30)]
        //[HorizontalGroup("Slider")]
        //public T min;

        ////[HideLabel]
        //[LabelWidth(30)]
        //[HorizontalGroup("Slider")]
        //public T max;

        ////[HorizontalGroup("Slider2")]
        //[PropertyRange("min", "max")]
        //[HideLabel]
        //public int testVal;

        //[MinMaxSlider("min", "max", true)]
        //public Vector2Int test;
    }
}