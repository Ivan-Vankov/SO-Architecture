#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
#endif
#endif
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static Vaflov.Config;

namespace Vaflov {
    public class Constant<T> : EditorScriptableObject<Constant<T>> {
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