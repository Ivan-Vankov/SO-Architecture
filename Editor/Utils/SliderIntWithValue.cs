using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Vaflov {
    public class SliderIntWithValue : SliderInt {

        public new class UxmlFactory : UxmlFactory<SliderIntWithValue, UxmlTraits> { }

        private readonly IntegerField intValue;

        public override int value {
            set {
                base.value = value;

                if (intValue != null) {
                    intValue.SetValueWithoutNotify(base.value);
                }
            }
        }

        public SliderIntWithValue() : this(null, 0, 10) {}

        public SliderIntWithValue(int start, int end, SliderDirection direction = SliderDirection.Horizontal, float pageSize = 0)
            : this(null, start, end, direction, pageSize) {
        }

        public SliderIntWithValue(string label, int start = 0, int end = 10, SliderDirection direction = SliderDirection.Horizontal, float pageSize = 0)
            : base(label, start, end, direction, pageSize) {

            intValue = new IntegerField();
            intValue.style.width = 30;
            intValue.style.flexGrow = 0;
            intValue.RegisterValueChangedCallback(evt => {
                value = evt.newValue;
            });

            Add(intValue);

            intValue.SetValueWithoutNotify(value);
        }
    }
}
