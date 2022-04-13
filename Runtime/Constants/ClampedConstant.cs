using System;

namespace Vaflov {
    public class ClampedConstant<T> : Constant<T> where T: IComparable, IComparable<T>, IEquatable<T> {
        public bool isClamped = false;
        public T min;
        public T max;
    }
}