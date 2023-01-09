using Microsoft.CSharp;
using Sirenix.OdinInspector;
using System;
using System.CodeDom;
using UnityEngine;
using static Vaflov.Config;

namespace Vaflov {
    public abstract class GameEvent2Base<V, T, U> : GameEventBase where V : GameEvent2Base<V, T, U> {
        [LabelWidth(80)]
        [PropertyOrder(13)]
        public bool showTypes;

        public static readonly CSharpCodeProvider codeProvider = new CSharpCodeProvider();
        public static readonly CodeTypeReference TTypeRef = new CodeTypeReference(typeof(T));
        public static readonly CodeTypeReference UTypeRef = new CodeTypeReference(typeof(U));

        [ShowInInspector]
        [HideLabel]
        //[LabelWidth(preferedEditorLabelWidth)]
        [HorizontalGroup("Types")]
        public string Arg1Type => codeProvider.GetTypeOutput(TTypeRef);

        [ShowInInspector]
        [HideLabel]
        //[LabelWidth(preferedEditorLabelWidth)]
        [HorizontalGroup("Types")]
        public string Arg2Type => codeProvider.GetTypeOutput(UTypeRef);

        public abstract void Raise(T arg1, U arg2);

        public virtual void AddListener(GameEventListener2Base<V, T, U> listener) {
            #if UNITY_EDITOR
            listeners.Add(listener);
            #endif
        }

        public virtual void RemoveListener(GameEventListener2Base<V, T, U> listener) {
            #if UNITY_EDITOR
            var index = listeners.IndexOf(listener);
            if (index > -1) {
                listeners[index] = listeners[listeners.Count - 1];
                listeners.RemoveAt(listeners.Count - 1);
            }
            #endif
        }
    }
}
