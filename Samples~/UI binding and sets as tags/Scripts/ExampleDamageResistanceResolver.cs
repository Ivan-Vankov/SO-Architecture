using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

namespace Vaflov {
    [Serializable]
    public struct DamageResistanceSet {
        public GameObjectSet objects;
        [Range(0, 100)] public int resistance;
    }

    [CreateAssetMenu(
        fileName = "Example Damage Resistance Resolver",
        menuName = "SO Architecture/Example/Example Damage Resistance Resolver")]
    public class ExampleDamageResistanceResolver : ScriptableObject {
        [ListDrawerSettings(Expanded = true)]
        public List<DamageResistanceSet> resistanceSets = new List<DamageResistanceSet>();

        public int CalcDamageResistance(GameObject gameObject) {
            var resistance = 0;
            foreach (var resistanceSet in resistanceSets) {
                if (resistanceSet.objects && resistanceSet.objects.Contains(gameObject)) {
                    resistance = Clamp(resistance + resistanceSet.resistance, 0, 100);
                }
            }
            return resistance;
        }
    }
}
