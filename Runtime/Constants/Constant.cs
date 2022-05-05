using UnityEngine;

namespace Vaflov {
    public class Constant<T> : ScriptableObject {
        [SerializeField] private T value = default;
        public T Value => value;
    }
}