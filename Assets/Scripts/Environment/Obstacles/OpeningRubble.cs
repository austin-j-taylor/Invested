using UnityEngine;
using System.Collections;

/*
 * A collection of colliders that, when hit with an object with the "OpeningRubble" tag, will collapse.
 */
public class OpeningRubble : MonoBehaviour {

    private void Awake() {
        Collider[] cols = GetComponentsInChildren<Collider>();
        foreach (Collider col in cols) {
            OpeningRubbleComponent rubble = col.gameObject.AddComponent<OpeningRubbleComponent>();
            rubble.parent = this;
        }
    }

    // Trigger all child colliders
    public void TriggerAll() {
        foreach (OpeningRubbleComponent rubbles in GetComponentsInChildren<OpeningRubbleComponent>()) {
            rubbles.Trigger();
        }
    }

    private class OpeningRubbleComponent : MonoBehaviour {

        public OpeningRubble parent;

        private void OnCollisionEnter(Collision collision) {
            if (collision.collider.CompareTag("OpeningRubble")) {
                parent.TriggerAll();
            }
        }

        public void Trigger() {
            GetComponent<Rigidbody>().isKinematic = false;
        }
    }

}
