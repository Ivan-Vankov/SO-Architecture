#if ODIN_INSPECTOR && UNITY_EDITOR
using Sirenix.OdinInspector.Editor;

namespace Vaflov {
    public class EmptyOdinMenuItem : OdinMenuItem {
        public EmptyOdinMenuItem(OdinMenuTree tree, string name, object value) : base(tree, name, value) {
            Style = new OdinMenuStyle() {
                Height = 0,
                Borders = false,
            };
        }

        public override void DrawMenuItem(int indentLevel) { }
    }
}
#endif