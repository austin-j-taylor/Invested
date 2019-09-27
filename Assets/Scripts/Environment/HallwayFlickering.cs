using UnityEngine;
using System.Collections;

public class HallwayFlickering : MonoBehaviour {

    [SerializeField]
    private float flickerPeriod = 1;
    [SerializeField]
    private bool instantlyOn = false;

    LightFlickering[] lightsToActivate;
    bool done = false;

    private void Awake() {
        lightsToActivate = GetComponentsInChildren<LightFlickering>();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger) { // => I am a trigger, and the player is not a trigger
            if (!done) {
                done = true;
                StartCoroutine(FlickerOnLights());
            }
        }
    }

    private IEnumerator FlickerOnLights() {
        foreach(LightFlickering light in lightsToActivate) {
            // Randomly flicker certain lights
            if(instantlyOn && Random.Range(0, 100) > 20) {
                light.On();
            } else {
                light.FlickerOn();
            }
            yield return new WaitForSeconds(flickerPeriod);
        }
    }
}
