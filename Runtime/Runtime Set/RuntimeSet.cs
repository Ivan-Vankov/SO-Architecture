using Sirenix.OdinInspector;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Vaflov.Config;

namespace Vaflov {
    public class RuntimeSet<T> : EditorScriptableObject<RuntimeSet<T>>, ICollection<T>, IEnumerable<T>, IEnumerable, ISerializationCallbackReceiver where T : UnityEngine.Object {
        [ReadOnly]
        [ListDrawerSettings(
            DraggableItems = false,
            Expanded = true,
            ShowPaging = true,
            NumberOfItemsPerPage = 20
            //ShowItemCount = false
            //HideRemoveButton = true
            )]
        [PropertyOrder(20)]
        public List<T> items = new List<T>();
        public Dictionary<T, int> itemToIdx = new Dictionary<T, int>();

        public event Action<T> OnItemAdded;
        public event Action<T> OnItemRemoved;

        public static readonly CodeTypeReference typeRef = new CodeTypeReference(typeof(T));

        #if ODIN_INSPECTOR
        [ShowInInspector]
        [ReadOnly]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(11)]
        #endif
        public string Type => codeProvider.GetTypeOutput(typeRef);

        public override string EditorToString() => $"{{ {typeof(T).Name} }}";

        public int Count => items.Count;

        // Lists can be serialized natively by unity => no custom serialization needed
        public void OnBeforeSerialize() { }

        // Fill dictionary with list contents
        public void OnAfterDeserialize() {
            itemToIdx.Clear();
            for (int i = 0; i < items.Count; ++i) {
                itemToIdx.Add(items[i], i);
            }
        }

        public void Add(T item) {
            if (!itemToIdx.ContainsKey(item)) {
                items.Add(item);
                itemToIdx[item] = items.Count - 1;
                OnItemAdded?.Invoke(item);
            }
        }

        public bool Remove(T item) {
            if (!itemToIdx.Remove(item, out int idx))
                return false;
            var lastObj = items[items.Count - 1];
            items[idx] = lastObj;
            items.RemoveAt(items.Count - 1);
            if (idx < items.Count) {
                itemToIdx[lastObj] = idx;
            }
            OnItemRemoved?.Invoke(item);
            return true;
        }

        public void Clear() {
            items.Clear();
            itemToIdx.Clear();
        }

        public bool Contains(T item) {
            return itemToIdx.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            items.CopyTo(array, arrayIndex);
        }

        public bool IsReadOnly => true;

        public IEnumerator<T> GetEnumerator() => items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
    }
}
