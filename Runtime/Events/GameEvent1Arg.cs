#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System;
using static Vaflov.Config;

namespace Vaflov {
    public class GameEvent1Arg<T> : GameEventBase {
        public Action<T> action;

        #if ODIN_INSPECTOR
        // TODO: Validate that argname has no whitespaces?
        //[LabelText("Arg Name")]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(13)]
        //[OnValueChanged(nameof(OnRangeChanged))]
        #endif
        public string argName;

        public void Raise(T arg1) {
            action?.Invoke(arg1);
        }
    }
}