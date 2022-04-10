using UnityEngine;

namespace Vaflov {
    [CreateAssetMenu(
        fileName = "Transform Game Event", 
        menuName = "SO Architecture/Events/Transform Game Event")]
    public class TransformGameEvent : GameEvent1Arg<Transform> { }
}