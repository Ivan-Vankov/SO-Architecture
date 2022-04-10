using UnityEngine;

namespace Vaflov {
    [CreateAssetMenu(
        fileName = "Color Game Event", 
        menuName = "SO Architecture/Events/Color Game Event")]
    public class ColorGameEvent : GameEvent1Arg<Color> { }
}