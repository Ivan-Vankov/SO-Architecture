#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
#endif

namespace Vaflov {
    [System.Serializable]
    public struct CustomData {
        public int a;
        public string b;
    }

    public delegate void Test2ArgAction(int testArg1, CustomData testArg2);

    //[UnityEngine.CreateAssetMenu(fileName = "Test", menuName = "SO Architecture/Test")]
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
        //[Button(Expanded = true)]
        [Button]
        [DrawButtonTypes]
        [PropertyOrder(15)]
        #endif
        public override void Raise(int testArg1, CustomData testArg2) {
            //var listeners = componentListeners.objects;
            //for (int i = 0; i < componentListeners.Count; ++i) {
            //    // TODO: Use Unsafe.As here, it is 1000x faster https://www.tabsoverspaces.com/233888-what-is-the-cost-of-casting-in-net-csharp
            //    //var lis = Unsafe.As<GameEventListener2Base<Test2ArgGameEvent, int, CustomData>>(listeners[i]);
            //    var typedListener = listeners[i] as GameEventListener<Test2ArgGameEvent, int, CustomData>;
            //    typedListener.CallResponse(testArg1, testArg2);
            //}
            // TODO: Do the same for so listeners
            action?.Invoke(testArg1, testArg2);
        }

        public override string EditorToString() {
            return "(testArg1, testArg2)";
        }

        //[Button(Expanded = true)]
        public void SetIconForObject(Texture2D icon) {
            //var type = typeof(EditorGUIUtility);
            //var setIconForObjectMethodInfo =
            //        type.GetMethod("SetIconForObject", BindingFlags.Static | BindingFlags.NonPublic);

            //setIconForObjectMethodInfo.Invoke(null, new object[] { this, icon });
            EditorGUIUtility.SetIconForObject(this, icon);
        }

        public override void AddListener(GameEventListener<Test2ArgGameEvent, int, CustomData> listener) {
            base.AddListener(listener);
            action += listener.CallResponse;
        }

        public override void RemoveListener(GameEventListener<Test2ArgGameEvent, int, CustomData> listener) {
            base.RemoveListener(listener);
            action -= listener.CallResponse;
        }

        public void Subscribe(Test2ArgAction action) {
            this.action += action;
        }

        public void Unsubscribe(Test2ArgAction action) {
            this.action -= action;
        }

        public static Test2ArgGameEvent operator +(Test2ArgGameEvent self, Test2ArgAction action) {
            self.action += action;
            return self;
        }

        public static Test2ArgGameEvent operator -(Test2ArgGameEvent self, Test2ArgAction action) {
            self.action += action;
            return self;
        }
    }
}
