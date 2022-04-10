using UnityEngine;

namespace Vaflov {
    [CreateAssetMenu(
        fileName = "GameObject Game Event", 
        menuName = "SO Architecture/Events/GameObject Game Event")]
    public class GameObjectGameEvent : GameEvent1Arg<GameObject> { }
}