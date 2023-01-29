#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Vaflov {
    public class GameObjectConstant : UnityObjectConstant<UnityEngine.GameObject> {
        public static Texture prefabIcon;

        public override Texture GetEditorIcon() {
            #if UNITY_EDITOR
            prefabIcon = prefabIcon == null ? EditorGUIUtility.FindTexture("Prefab Icon") : prefabIcon;
            if (Value == null) {
                return prefabIcon;
            }
            var root = PrefabUtility.GetNearestPrefabInstanceRoot(Value);
            return root != null
                ? PrefabUtility.GetIconForGameObject(root)
                : prefabIcon;
            #else
            return null;
            #endif
        }
    }
}
