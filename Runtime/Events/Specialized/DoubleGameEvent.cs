using UnityEngine;

namespace Vaflov {
    [CreateAssetMenu(
        fileName = "Double Game Event", 
        menuName = "SO Architecture/Events/Double Game Event")]
    public class DoubleGameEvent : GameEvent1Arg<double> { }
}