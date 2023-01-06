using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using UnityEngine;
using static UnityEngine.Mathf;

namespace Vaflov {
    public static class ComponentUtil {
        public static void ReplaceComponentWith(this Component toReplace, Component toReplaceWith) {
            #if UNITY_EDITOR
            var components = toReplace.GetComponents<Component>();
            var expanded = new HashSet<Component>();
            foreach (var component in components) {
                if (InternalEditorUtility.GetIsInspectorExpanded(component)) {
                    expanded.Add(component);
                }
            }
            #endif
            var index = toReplace.GetComponentIndexInGameObject();
            toReplaceWith.MoveComponentToIndexInGameObject(index);
            UnityEngine.Object.DestroyImmediate(toReplace);
            #if UNITY_EDITOR
            foreach (var component in components) {
                if (component != null) {
                    InternalEditorUtility.SetIsInspectorExpanded(component, expanded.Contains(component));
                }
            }
            InternalEditorUtility.SetIsInspectorExpanded(toReplaceWith, true);
            ActiveEditorTracker.sharedTracker.ForceRebuild();
            #endif
        }

        public static void MoveComponentToIndexInGameObject(this Component component, int index) {
            var currentIndex = component.GetComponentIndexInGameObject();
            #if UNITY_EDITOR
            Func<Component, bool> moveFunc = currentIndex > index
                ? ComponentUtility.MoveComponentUp
                : ComponentUtility.MoveComponentDown;
            var moveSteps = Abs(currentIndex - index);
            for (int i = 0; i < moveSteps; ++i) {
                moveFunc(component);
            }
            #endif
        }

        public static int GetComponentIndexInGameObject(this Component component) {
            var components = component.GetComponents<Component>();
            for (int i = 0; i < components.Length; ++i) {
                if (components[i] == component) {
                    return i;
                }
            }
            return -1;
        }
    }
}
