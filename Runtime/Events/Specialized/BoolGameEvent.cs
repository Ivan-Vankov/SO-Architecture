using UnityEngine;

namespace Vaflov {
    [CreateAssetMenu(
        fileName = "Bool Game Event", 
        menuName = "SO Architecture/Events/Bool Game Event")]
    public class BoolGameEvent : GameEvent1Arg<bool> { }
}