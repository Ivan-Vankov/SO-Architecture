using System;
using System.Diagnostics;
using UnityEngine;

namespace Vaflov {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class AnimParamAttribute : Attribute {
        public string animator;
        public AnimatorControllerParameterType type;

        public AnimParamAttribute(string animator, AnimatorControllerParameterType type) {
            this.animator = animator;
            this.type = type;
        }
    }
}
