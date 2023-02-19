#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace Vaflov {
    [HideInPlayMode]
    [HideLabel]
    [Serializable]
    public class GameEventEditMenu { }

    [Serializable]
    public class ObjSet<T> : ISerializationCallbackReceiver where T : UnityEngine.Object {
        [HideInInspector]
        public string name = "Objects";

        #if ODIN_INSPECTOR
        [ReadOnly]
        [LabelText("$" + nameof(name))]
        [ListDrawerSettings(
            DraggableItems = false,
            //Expanded = true,
            ShowPaging = true,
            NumberOfItemsPerPage = 10
            //IsReadOnly = true
            //ShowItemCount = false
            //HideRemoveButton = true
            )]
        #endif
        public List<T> objects = new List<T>();
        public Dictionary<T, int> objToIdxs = new Dictionary<T, int>();

        // Lists can be serialized natively by unity => no custom serialization needed
        public void OnBeforeSerialize() { }

        // Fill dictionary with list contents
        public void OnAfterDeserialize() {
            objToIdxs.Clear();
            for (int i = 0; i < objects.Count; ++i) {
                objToIdxs.Add(objects[i], i);
            }
        }

        public void Add(T listener) {
            //if (!objects.Contains(listener))
            //    objects.Add(listener);
            if (!objToIdxs.ContainsKey(listener)) {
                objects.Add(listener);
                objToIdxs[listener] = objects.Count - 1;
            }
        }

        public bool Remove(T obj) {
            //objects.Remove(obj);
            if (!objToIdxs.Remove(obj, out int idx))
                return false;
            var lastObj = objects[objects.Count - 1];
            objects[idx] = lastObj;
            objects.RemoveAt(objects.Count - 1);
            if (idx < objects.Count) {
                objToIdxs[lastObj] = idx;
            }
            return true;
        }

        public bool Contains(T obj) {
            return objToIdxs.ContainsKey(obj);
        }

        public void Clear() {
            objects.Clear();
            objToIdxs.Clear();
        }

        public void CopyTo(T[] array, int arrayIndex) {
            objects.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator() => objects.GetEnumerator();

        public int Count => objects.Count;

        public bool IsReadOnly => true;
    }

    public class GameEventBase : EditorScriptableObject {
        #if UNITY_EDITOR
        #if ODIN_INSPECTOR
        [HideLabel]
        [FoldoutGroup("Listeners", true)]
        [PropertyOrder(30)]
        #endif
        public ObjSet<Component> componentListeners = new ObjSet<Component>() { name = "Component Listeners" };
        #endif

        #if UNITY_EDITOR
        #if ODIN_INSPECTOR
        [HideLabel]
        [FoldoutGroup("Listeners", true)]
        [PropertyOrder(31)]
        #endif
        public ObjSet<ScriptableObject> SOListeners = new ObjSet<ScriptableObject>() { name = "SO Listeners" };
        #endif

        public override Type EditorObjectBaseType => typeof(GameEventBase);

        #if UNITY_EDITOR
        public void AddListener(GameEventListenerBase listener) {
            if (listener.parent is MonoBehaviour) {
                componentListeners.Add(Unsafe.As<MonoBehaviour>(listener.parent));
            } else {
                SOListeners.Add(Unsafe.As<ScriptableObject>(listener.parent));
            }
        }

        public void RemoveListener(GameEventListenerBase listener) {
            if (listener.parent is MonoBehaviour monoBehaviour) {
                componentListeners.Remove(monoBehaviour);
            } else if (listener.parent is ScriptableObject scriptableObject) {
                SOListeners.Remove(scriptableObject);
            } else {
                Debug.Assert(false, "Unknown game event listener object");
            }
        }
        #endif

        [PropertyOrder(40)]
        public GameEventEditMenu editMenu;
    }
}
