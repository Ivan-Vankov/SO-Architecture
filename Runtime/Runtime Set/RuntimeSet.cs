using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vaflov {
    public class RuntimeSet<T> : MonoBehaviour, ICollection<T>, IEnumerable<T>, IEnumerable, ISerializationCallbackReceiver where T : UnityEngine.Object {
        public event Action<T> OnItemAdded;
        public event Action<T> OnItemRemoved;

        [ReadOnly]
        [ListDrawerSettings(
            DraggableItems = false,
            //Expanded = true,
            ShowPaging = true,
            NumberOfItemsPerPage = 20
            //ShowItemCount = false
            //HideRemoveButton = true
            )]
        public List<T> objects = new List<T>();
        public Dictionary<T, int> objToIdxs = new Dictionary<T, int>();

        public int Count => objects.Count;

        // Lists can be serialized natively by unity => no custom serialization needed
        public void OnBeforeSerialize() { }

        // Fill dictionary with list contents
        public void OnAfterDeserialize() {
            objToIdxs.Clear();
            for (int i = 0; i < objects.Count; ++i) {
                objToIdxs.Add(objects[i], i);
            }
        }

        public void Add(T item) {
            //if (!objects.Contains(item))
            //    objects.Add(item);
            if (!objToIdxs.ContainsKey(item)) {
                objects.Add(item);
                objToIdxs[item] = objects.Count - 1;
                OnItemAdded?.Invoke(item);
            }
        }

        public bool Remove(T item) {
            // objects.Remove(obj);
            if (!objToIdxs.Remove(item, out int idx))
                return false;
            var lastObj = objects[objects.Count - 1];
            objects[idx] = lastObj;
            objects.RemoveAt(objects.Count - 1);
            if (idx < objects.Count) {
                objToIdxs[lastObj] = idx;
            }
            OnItemRemoved?.Invoke(item);
            return true;
        }

        public void Clear() {
            objects.Clear();
            objToIdxs.Clear();
        }

        public bool Contains(T item) {
            return objToIdxs.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            objects.CopyTo(array, arrayIndex);
        }

        public bool IsReadOnly => true;

        public IEnumerator<T> GetEnumerator() => objects.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => objects.GetEnumerator();
    }
}
