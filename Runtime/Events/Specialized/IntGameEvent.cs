using UnityEngine;

namespace Vaflov {
    [CreateAssetMenu(
        fileName = "Int Game Event", 
        menuName = "SO Architecture/Events/Int Game Event")]
    public class IntGameEvent : GameEvent1Arg<int> { }
}