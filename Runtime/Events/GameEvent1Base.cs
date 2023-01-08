using Microsoft.CSharp;
using Sirenix.OdinInspector;
using System.CodeDom;

namespace Vaflov {
    public abstract class GameEvent1Base<V, T> : GameEventBase where V : GameEvent1Base<V, T> {
        public static readonly CSharpCodeProvider codeProvider = new CSharpCodeProvider();
        public static readonly CodeTypeReference TTypeRef = new CodeTypeReference(typeof(T));

        [ShowInInspector]
        [HideLabel]
        //[LabelWidth(preferedEditorLabelWidth)]
        public string Arg1Type => codeProvider.GetTypeOutput(TTypeRef);

        public abstract void Raise(T arg1);

        public virtual void AddListener(GameEventListener1Base<V, T> listener) {
            #if UNITY_EDITOR
            listeners.Add(listener);
            #endif
        }

        public virtual void RemoveListener(GameEventListener1Base<V, T> listener) {
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
