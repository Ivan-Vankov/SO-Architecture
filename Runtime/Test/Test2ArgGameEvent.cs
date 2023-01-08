#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Vaflov {
    [System.Serializable]
    public struct CustomData {
        public int a;
        public string b;
    }

    public delegate void Test2ArgAction(int testArg1, CustomData testArg2);

    [UnityEngine.CreateAssetMenu(fileName = "Test", menuName = "SO Architecture/Test")]
    public class Test2ArgGameEvent : GameEvent2Base<Test2ArgGameEvent, int, CustomData> {
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

        #if ODIN_INSPECTOR
        //[ShowDrawerChain]
        [Button(Expanded = true)]
        [PropertyOrder(15)]
        #endif
        public override void Raise(int testArg1, CustomData testArg2) {
            action?.Invoke(testArg1, testArg2);
        }

        public override void AddListener(GameEventListener2Base<Test2ArgGameEvent, int, CustomData> listener) {
            base.AddListener(listener);
            action += listener.CallResponse;
        }

        public override void RemoveListener(GameEventListener2Base<Test2ArgGameEvent, int, CustomData> listener) {
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
