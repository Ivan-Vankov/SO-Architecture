using UnityEngine;

namespace Vaflov {
    [CreateAssetMenu(
        fileName = "AnimationCurve Game Event", 
        menuName = "SO Architecture/Events/AnimationCurve Game Event")]
    public class AnimationCurveGameEvent : GameEvent1Arg<AnimationCurve> { }
}
