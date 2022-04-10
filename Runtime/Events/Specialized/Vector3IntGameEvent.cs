using UnityEngine;

namespace Vaflov {
    [CreateAssetMenu(
        fileName = "Vector3Int Game Event", 
        menuName = "SO Architecture/Events/Vector3Int Game Event")]
    public class Vector3IntGameEvent : GameEvent1Arg<Vector3Int> { }
}