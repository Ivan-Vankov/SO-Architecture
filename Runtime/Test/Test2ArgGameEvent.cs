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
    public class Test2ArgGameEvent : GameEvent2Base<int, CustomData, Test2ArgGameEvent> {
    //public class Test2ArgGameEvent : GameEventBase {
        public const string arg1Name = "testArg1";
        public const string arg2Name = "testArg2";
        public delegate void Test2ArgAction(int testArg1, CustomData testArg2);
        public event Test2ArgAction action;

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
        public override void Raise(int testArg1, CustomData testArg2) {
            action?.Invoke(testArg1, testArg2);
        }

        public override void AddListener(GameEventListener2Base<int, CustomData, Test2ArgGameEvent> listener) {
            base.AddListener(listener);
            action += listener.CallResponse;
        }

        public override void RemoveListener(GameEventListener2Base<int, CustomData, Test2ArgGameEvent> listener) {
            base.RemoveListener(listener);
            action -= listener.CallResponse;
        }

        //public static Test2ArgGameEvent operator +(Test2ArgGameEvent self, Test2ArgAction action) {
        //    self.action += action;
        //    return self;
        //}

        //public static Test2ArgGameEvent operator -(Test2ArgGameEvent self, Test2ArgAction action) {
        //    self.action += action;
        //    return self;
        //}
    }
}
