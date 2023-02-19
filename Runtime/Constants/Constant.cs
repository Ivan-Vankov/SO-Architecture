#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.Serialization;
#endif
using System;
using System.CodeDom;
using UnityEngine;
using static Vaflov.SOArchitectureConfig;

namespace Vaflov {
    public class Constant<T> : EditorScriptableObject {
        [SerializeField]
        #if ODIN_INSPECTOR
        [HideLabel]
        [PropertyOrder(20)]
        #endif
        private T value = default;
        public T Value => value;

        public static readonly CodeTypeReference typeRef = new CodeTypeReference(typeof(T));

        #if ODIN_INSPECTOR
        [ShowInInspector]
        [ReadOnly]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(11)]
        #endif
        public string Type => codeProvider.GetTypeOutput(typeRef);

        public override Type EditorObjectBaseType => typeof(Constant<>);

        public override string EditorToString() {
            if (Value == null) {
                return "null";
            }
            var str = Value.ToString();
            if (str.Length > 100) {
                str = "...";
            } else {
                str = str.Replace("\n", " ");
            }
            return str;
        }
    }
}