using Sirenix.OdinInspector;
using UnityEngine;
using static UnityEngine.Mathf;

namespace Vaflov {
    public class ExampleDamageable : MonoBehaviour {
        public int maxHealth = 100;
        public int health = 100;

        [InlineEditor]
        public ExampleDamageResistanceResolver damageResistanceResolver;
        public OnDamageTakenGameEvent onDamageTaken;

        public void TakeDamage(int damage) {
            var resistance = damageResistanceResolver.CalcDamageResistance(gameObject);
            damage = damage * (100 - resistance) / 100;
            health = Clamp(health - damage, 0, maxHealth);
            var healthPct = (float)health / maxHealth;
            onDamageTaken.Raise(this, healthPct);
        }

        public void OnTriggerEnter2D(Collider2D collision) {
            // If you hit another damageable
            if (collision.GetComponent<ExampleDamageable>()) {
                TakeDamage(20);
            }
        }
    }
}
