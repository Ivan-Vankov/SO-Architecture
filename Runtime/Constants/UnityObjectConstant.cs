#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static Vaflov.SOArchitectureConfig;

namespace Vaflov {
    #if ODIN_INSPECTOR
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
    #endif

    public static class UnityObjectConstant {
        public static Action OnUnityObjectConstantChanged;
    }

    public class UnityObjectConstant<T> : Constant<T> where T : UnityEngine.Object {
        #if ODIN_INSPECTOR
        [OnValueChanged(nameof(OnUnityObjectConstantChanged))]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(16)]
        public InlineEditorModes editorMode = InlineEditorModes.GUIOnly;
        #endif

        #if UNITY_EDITOR
        public override Texture GetEditorIcon() {
            return EditorGUIUtility.ObjectContent(Value, typeof(T)).image;
        }
        #endif

        public override string EditorToString() {
            return base.EditorToString().Replace("UnityEngine.", "");
        }

        public void OnUnityObjectConstantChanged() {
            UnityObjectConstant.OnUnityObjectConstantChanged?.Invoke();
        }

        #if ODIN_INSPECTOR && UNITY_EDITOR
        public override List<OdinContextMenuItem> GetContextMenuItems() {
            var items = base.GetContextMenuItems();
            items.Add(new OdinContextMenuItem("Select in project", () => {
                if (Value == null) { return; }
                Selection.activeObject = Value;
                EditorGUIUtility.PingObject(Selection.activeObject);
            }, icon: SdfIconType.HandIndexThumb));
            return items;
        }
        #endif
    }
}
