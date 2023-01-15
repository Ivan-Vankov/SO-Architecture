#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using UnityEditor;
#endif
using UnityEngine;

namespace Vaflov {
    [CreateAssetMenu(
        fileName = "Game Event Listener",
        menuName = "SO Architecture/Game Event Listener",
        order = 30)]
    [DefaultExecutionOrder(-2000)]
    public class GameEventListenerSO : ScriptableObject {
        #if ODIN_INSPECTOR
        [AssetsOnly]
        [OnValueChanged(nameof(RefreshListener))]
        [Required]
        #endif
        public GameEventBase eventRef;

        #if ODIN_INSPECTOR
        [PropertyOrder(10)]
        //[InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        [InlineEditor]
        #endif
        public GameEventListenerBase listener;

        [Button]
        public void RemoveListener() {
            if (listener) {
                //Debug.Log(AssetDatabase.GetAssetPath(listener));
                AssetDatabase.RemoveObjectFromAsset(listener);
                //Debug.Log(AssetDatabase.GetAssetPath(listener));
                DestroyImmediate(listener, true);
                EditorUtility.SetDirty(this);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
                //AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(listener));
            }
        }

        public void RefreshListener() {
            Debug.Log("Refresh");
            //var saveAssets = false;
            if (listener) {
                //AssetDatabase.RemoveObjectFromAsset(listener);
                AssetDatabase.Refresh();
                DestroyImmediate(listener, true);
                listener = null;
                //saveAssets = true;
                //AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
                AssetDatabase.ForceReserializeAssets(new[] { AssetDatabase.GetAssetPath(this) });
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                //EditorUtility.SetDirty(this);
            }
            listener = GetListenerInstance();
            if (listener) {
                listener.parent = this;
                listener.AssignGameEvent(eventRef);
                //AssetDatabase.CreateAsset(listener, "Assets/Listener.asset");
                listener.name = "Listener";
                AssetDatabase.Refresh();
                AssetDatabase.AddObjectToAsset(listener, this);
                //AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(listener), ImportAssetOptions.ForceUpdate);
                //AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(listener));
                //saveAssets = true;
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                //EditorUtility.SetDirty(this);
                //EditorUtility.SetDirty(listener);
                //AssetDatabase.SaveAssets();
            }
            //if (saveAssets) {
            //    AssetDatabase.SaveAssets();
            //    EditorUtility.SetDirty(this);
            //}
        }

        public GameEventListenerBase GetListenerInstance() {
            return GameEventListenerUtil.GetListenerInstance(eventRef);
        }

        private void OnEnable() {
            Debug.Log("Enable");
            if (listener) {
                Debug.Log("Init");
                listener.OnInit();
            }
        }

        private void OnDisable() {
            Debug.Log("Disable");
            if (listener) {
                Debug.Log("Done");
                listener.OnDone();
            }
        }
    }
}
