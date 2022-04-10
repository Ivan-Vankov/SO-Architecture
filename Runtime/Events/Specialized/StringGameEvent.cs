using UnityEngine;

namespace Vaflov {
    [CreateAssetMenu(
        fileName = "String Game Event", 
        menuName = "SO Architecture/Events/String Game Event")]
    public class StringGameEvent : GameEvent1Arg<string> { }
}