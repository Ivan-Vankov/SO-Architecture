using UnityEngine;

namespace Vaflov {
    [CreateAssetMenu(
        fileName = "Vector2Int Game Event", 
        menuName = "SO Architecture/Events/Vector2Int Game Event")]
    public class Vector2IntGameEvent : GameEvent1Arg<Vector2Int> { }
}