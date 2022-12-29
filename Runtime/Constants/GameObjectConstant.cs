using UnityEditor;
using UnityEngine;

namespace Vaflov {
    public class GameObjectConstant : UnityObjectConstant<UnityEngine.GameObject> {
        public static Texture prefabIcon;

        public override Texture GetEditorIcon() {
            prefabIcon = prefabIcon == null ? EditorGUIUtility.FindTexture("Prefab Icon") : prefabIcon;
            if (Value == null) {
                return prefabIcon;
            }
            var root = PrefabUtility.GetNearestPrefabInstanceRoot(Value);
            return root != null
                ? PrefabUtility.GetIconForGameObject(root)
                : prefabIcon;
        }
    }
}
