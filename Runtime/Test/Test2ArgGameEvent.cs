using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Vaflov {
    [Serializable]
    public struct CustomData {
        public int a;
        public string b;
    }

    [CreateAssetMenu(fileName = "Test", menuName = "SOArchitecture/Test")]
    //public class Test2ArgGameEvent : GameEvent2Base<int, CustomData> {
    public class Test2ArgGameEvent : GameEventBase {
        public delegate void Test2ArgEvent(int testArg1, CustomData testArg2);
        public event Test2ArgEvent action;

        //#if ODIN_INSPECTOR
        //// TODO: Validate that argname has no whitespaces?
        //[LabelText("Arg1")]
        //[LabelWidth(30)]
        //[PropertyOrder(13)]
        //[HorizontalGroup("Names")]
        ////[OnValueChanged(nameof(OnRangeChanged))]
        //#endif
        //public string Arg1;

        //#if ODIN_INSPECTOR
        //// TODO: Validate that argname has no whitespaces?
        //[LabelText("Arg2")]
        //[LabelWidth(30)]
        //[PropertyOrder(14)]
        //[HorizontalGroup("Names")]
        ////[OnValueChanged(nameof(OnRangeChanged))]
        //#endif
        //public string Arg2;

        [Button(Expanded = true)]
        [PropertyOrder(15)]
        public void Raise(int testArg1, CustomData testArg2) {
            action?.Invoke(testArg1, testArg2);
        }
    }
}
