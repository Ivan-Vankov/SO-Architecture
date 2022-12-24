using Sirenix.OdinInspector;
using System;

namespace Vaflov {
    public enum ObjectPreviewKind {
        None,
        InlineEditor,
        PreviewField
    }

    public static class UnityObjectConstantEditorEvents {
        public static Action OnUnityObjectConstantChanged;
    }

    public class UnityObjectConstant<T> : Constant<T> where T : UnityEngine.Object {
        [OnValueChanged(nameof(OnUnityObjectConstantChanged))]
        [PropertyOrder(15)]
        public ObjectPreviewKind preview = ObjectPreviewKind.InlineEditor;

        [ShowIf(nameof(IsInlineEditor))]
        [OnValueChanged(nameof(OnUnityObjectConstantChanged))]
        [PropertyOrder(16)]
        public InlineEditorModes editorMode = InlineEditorModes.GUIOnly;

        [ShowIf(nameof(IsPreviewField))]
        [OnValueChanged(nameof(OnUnityObjectConstantChanged))]
        [PropertyOrder(16)]
        public ObjectFieldAlignment alignment = ObjectFieldAlignment.Right;

        //[AssetsOnly]
        //[InlineEditor(InlineEditorModes.FullEditor)]
        //[PreviewField(ObjectFieldAlignment.)]
        //public GameObject SomePrefab;
        public bool IsInlineEditor() {
            return preview == ObjectPreviewKind.InlineEditor;
        }

        public bool IsPreviewField() {
            return preview == ObjectPreviewKind.PreviewField;
        }

        public void OnUnityObjectConstantChanged() {
            UnityObjectConstantEditorEvents.OnUnityObjectConstantChanged?.Invoke();
        }
    }
}
