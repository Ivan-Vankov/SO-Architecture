#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static Vaflov.UnityObjectUtil;
using static Vaflov.SOArchitectureConfig;

namespace Vaflov {
    [DefaultExecutionOrder(-2000)]
    public class GameEventListener : MonoBehaviour {
        #if ODIN_INSPECTOR
        [AssetsOnly]
        [LabelWidth(preferedEditorLabelWidth)]
        [Required]
        #endif
        public GameEventBase eventRef;

        [HideInInspector]
        public GameEventListenerBase listener;

        #if ODIN_INSPECTOR
        [PropertyOrder(10)]
        [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        [ShowInInspector]
        [EnableGUI]
        #endif
        public GameEventListenerBase Listener {
            get {
                #if UNITY_EDITOR
                if (listener && listener.GetEventRef() != eventRef) {
                    listener = null;
                }
                if (!listener && eventRef) {
                    listener = GameEventListenerUtil.GetListenerInstance(eventRef);
                    if (listener) {
                        listener.parent = this;
                        listener.AssignGameEvent(eventRef);
                        listener.name = "Listener";
                    }
                }
                var isPrefab = PrefabUtility.IsPartOfPrefabAsset(this);
                if (isPrefab) {
                    if (listener && !IsSavedAsAsset(listener)) {
                        var path = AssetDatabase.GetAssetPath(gameObject);
                        var allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                        foreach (var asset in allAssets) {
                            if (asset != gameObject
                                && AssetDatabase.IsSubAsset(asset)
                                && asset is GameEventListenerBase) {
                                DestroyImmediate(asset, true);
                            }
                        }
                        AssetDatabase.AddObjectToAsset(listener, this);
                        AssetDatabase.SaveAssets();
                    }
                } else {
                    if (listener && IsSavedAsAsset(listener)) {
                        listener = Instantiate(listener);
                    }
                }
                #endif
                if (listener) {
                    listener.parent = this;
                }
                return listener;
            }
        }

        private void OnEnable() {
            var listener = Listener;
            if (listener) {
                listener.OnInit();
            }
        }

        private void OnDisable() {
            var listener = Listener;
            if (listener) {
                listener.OnDone();
            }
        }
    }
}
