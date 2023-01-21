using UnityEngine;
using UnityEngine.UI;

namespace Vaflov {
    public class ExampleHealthBar : MonoBehaviour {
        public Image healthBarFill;

        public void UpdateHealthBar(float healthPct) {
            healthBarFill.fillAmount = healthPct;
        }
    }
}
