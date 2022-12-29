using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Vaflov.Config;

namespace Vaflov {
    public static class UnityObjectContantConfig {
        public static readonly Dictionary<Type, InlineEditorModes> defaultInlineEditorModes = new Dictionary<Type, InlineEditorModes>() {
            { typeof(Texture), InlineEditorModes.GUIAndPreview },
            { typeof(Texture2D), InlineEditorModes.GUIAndPreview },
            { typeof(Texture3D), InlineEditorModes.GUIAndPreview },
            { typeof(Texture2DArray), InlineEditorModes.GUIAndPreview },
            { typeof(AudioClip), InlineEditorModes.LargePreview },
        };
        public static readonly HashSet<Type> inlineEditorModesBlacklist = new HashSet<Type>() {
            typeof(GameObject),
        };
    }

    public static class UnityObjectConstantEditorEvents {
        public static Action OnUnityObjectConstantChanged;
    }

    public class UnityObjectConstant<T> : Constant<T> where T : UnityEngine.Object {
        [OnValueChanged(nameof(OnUnityObjectConstantChanged))]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(16)]
        public InlineEditorModes editorMode = InlineEditorModes.GUIOnly;

        public override Texture GetEditorIcon() {
            return EditorGUIUtility.ObjectContent(Value, typeof(T)).image;
        }

        public void OnUnityObjectConstantChanged() {
            UnityObjectConstantEditorEvents.OnUnityObjectConstantChanged?.Invoke();
        }

        #if ODIN_INSPECTOR && UNITY_EDITOR
        [Button(Icon = SdfIconType.HandIndexThumbFill)]
        [PropertyOrder(20)]
        public void SelectInProject() {
            if (Value == null) { return; }
            Selection.activeObject = Value;
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
        #endif
    }
}
