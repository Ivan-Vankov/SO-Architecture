using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Vaflov {
    public class SyncToSets : MonoBehaviour {
        #if ODIN_INSPECTOR
        #if UNITY_EDITOR
        [OnCollectionChanged(nameof(Before), null)]
        #endif
        [ListDrawerSettings(
            Expanded = true,
            ShowPaging = true,
            NumberOfItemsPerPage = 10
            )]
        #endif
        public List<GameObjectSet> sets = new List<GameObjectSet>();

        #if ODIN_INSPECTOR && UNITY_EDITOR
        public void Before(CollectionChangeInfo info) {
            if (!Application.isPlaying || !isActiveAndEnabled)
                return;
            GameObjectSet set = null;
            var add = true;

            if (info.ChangeType == CollectionChangeType.Add || info.ChangeType == CollectionChangeType.Insert) {
                set = info.Value as GameObjectSet;
            } else if (info.ChangeType == CollectionChangeType.RemoveIndex || info.ChangeType == CollectionChangeType.RemoveValue) {
                set = info.ChangeType == CollectionChangeType.RemoveIndex
                    ? sets[info.Index]
                    : info.Value as GameObjectSet;
                add = false;
            }

            if (set == null)
                return;
            if (add) {
                set.Add(gameObject);
            } else {
                set.Remove(gameObject);
            }
        }
        #endif

            private void OnEnable() {
            for (int i = 0; i < sets.Count; ++i) {
                var set = sets[i];
                if (set) {
                    set.Add(gameObject);
                }
            }
        }

        private void OnDisable() {
            for (int i = 0; i < sets.Count; ++i) {
                var set = sets[i];
                if (set) {
                    set.Remove(gameObject);
                }
            }
        }
    }
}
