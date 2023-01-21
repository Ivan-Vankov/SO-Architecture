using UnityEngine;

namespace Vaflov {
    public static class ExampleHealthBarUpdater {
        public static void UpdateHealthBars(ExampleDamageable damageable, float healthPct) {
            var healthBar = damageable.GetComponentInChildren<ExampleHealthBar>();
            if (healthBar) {
                healthBar.UpdateHealthBar(healthPct);
            }
        }
    }
}
