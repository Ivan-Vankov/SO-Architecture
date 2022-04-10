using UnityEngine;

namespace Vaflov {
    public class Constant<T> : ScriptableObject {
        [SerializeField] private T value;
        public T Value => value;
    }
}