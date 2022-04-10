using UnityEngine;

namespace Vaflov {
    [CreateAssetMenu(
        fileName = "Vector3 Game Event", 
        menuName = "SO Architecture/Events/Vector3 Game Event")]
    public class Vector3GameEvent : GameEvent1Arg<Vector3> { }
}