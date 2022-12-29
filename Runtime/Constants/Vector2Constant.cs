using Sirenix.OdinInspector;
using UnityEngine;

namespace Vaflov {
    public class Vector2Constant : RangeConstant<Vector2, float> {
        [Button]
        public void Test() {
            Debug.Log(typeof(UnityEngine.Object).IsAssignableFrom(typeof(UnityEngine.GameObject)));
        }
    }
}
