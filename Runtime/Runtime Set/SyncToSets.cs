#if ODIN_INSPECTOR
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif
using Sirenix.OdinInspector;
#endif
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Vaflov {
    public class SyncToSets : MonoBehaviour {
        [HideInInspector]
        public bool syncGameObject = true;

        #if ODIN_INSPECTOR
        [ShowInInspector]
        [PropertyOrder(3)]
        #endif
        public bool SyncGameObject {
            get => syncGameObject;
            set {
                #if UNITY_EDITOR
                Undo.RecordObject(this, "Reset GameObject Sync");
                #endif
                if (!objectToSync || objectToSync.GetType() != typeof(GameObject)) {
                    sets.Clear();
                }
                syncGameObject = value;
            }
        }

        [HideInInspector]
        public UnityEngine.Object objectToSync;

        #if ODIN_INSPECTOR
        [Required]
        [ShowInInspector]
        [HideIf("@this." + nameof(syncGameObject))]
        [PropertyOrder(3)]
        #endif
        public UnityEngine.Object ObjectToSync {
            get => objectToSync;
            set {
                #if UNITY_EDITOR
                Undo.RecordObject(this, "Reset Sync Object");
                #endif
                if (!objectToSync || !value || objectToSync.GetType() != value.GetType()) {
                    sets.Clear();
                }
                objectToSync = value;
            }
        }

        #if ODIN_INSPECTOR
        #if UNITY_EDITOR
        [OnCollectionChanged(nameof(Before), null)]
        #endif
        [RuntimeSetObjectPicker(nameof(GetRuntimeSetInnerType))]
        [HideIf("@this." + nameof(objectToSync) + " == null && !this." + nameof(syncGameObject))]
        [PropertyOrder(5)]
        [ListDrawerSettings(
            Expanded = true,
            ShowPaging = true,
            NumberOfItemsPerPage = 10
            )]
        #endif
        public List<RuntimeSetBase> sets = new List<RuntimeSetBase>();

        public Type GetRuntimeSetInnerType() {
            if (syncGameObject) {
                return typeof(GameObject);
            }
            return objectToSync == null ? null : objectToSync.GetType();
        }

        #if ODIN_INSPECTOR && UNITY_EDITOR
        public void Before(CollectionChangeInfo info) {
            if (!Application.isPlaying || !isActiveAndEnabled)
                return;
            RuntimeSetBase set = null;
            var add = true;

            if (info.ChangeType == CollectionChangeType.Add || info.ChangeType == CollectionChangeType.Insert) {
                set = info.Value as RuntimeSetBase;
            } else if (info.ChangeType == CollectionChangeType.RemoveIndex || info.ChangeType == CollectionChangeType.RemoveValue) {
                set = info.ChangeType == CollectionChangeType.RemoveIndex
                    ? sets[info.Index]
                    : info.Value as RuntimeSetBase;
                add = false;
            }

            if (set == null)
                return;
            Action<RuntimeSetBase> func = add ? AddToSet : RemoveFromSet;
            func(set);
        }
        #endif

        public void AddToSet(RuntimeSetBase set) {
            if (set) {
                set.Add(syncGameObject ? gameObject : objectToSync);
            }
        }

        public void RemoveFromSet(RuntimeSetBase set) {
            if (set) {
                set.Remove(syncGameObject ? gameObject : objectToSync);
            }
        }

        private void OnEnable() {
            for (int i = 0; i < sets.Count; ++i) {
                AddToSet(sets[i]);
            }
        }

        private void OnDisable() {
            for (int i = 0; i < sets.Count; ++i) {
                RemoveFromSet(sets[i]);
            }
        }
    }
}