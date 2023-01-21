using UnityEngine;

namespace Vaflov {
    public class ExamplePlayer : MonoBehaviour {
        [Range(1, 100)]
        public float moveSpeed = 5;

        public void Update() {
            var xinput = Input.GetAxis("Horizontal");
            var yinput = Input.GetAxis("Vertical");

            transform.Translate(moveSpeed * Time.deltaTime * new Vector3(xinput, yinput));
        }
    }
}