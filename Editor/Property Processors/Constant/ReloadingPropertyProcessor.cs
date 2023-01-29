#if ODIN_INSPECTOR && UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;

namespace Vaflov {
    public abstract class ReloadingPropertyProcessor<T> : OdinPropertyProcessor<T>, IDisposable {
        public abstract ref Action ReloadAction { get; }

        protected override void Initialize() {
            ReloadAction += RefreshGUI;
        }

        public virtual void Dispose() {
            ReloadAction -= RefreshGUI;
        }

        public void RefreshGUI() {
            Property.RefreshSetup();
            GUIHelper.RequestRepaint();
        }
    }
}
#endif