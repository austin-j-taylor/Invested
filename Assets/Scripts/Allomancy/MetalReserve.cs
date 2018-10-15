using UnityEngine;

/*
 * Represents the metal reserve residing in an allomancer's stomach.
 */
public class MetalReserve : MonoBehaviour {

    private const float maxCapacity = 500; // stomach can hold up to .5kg of metal, why not
    
    private float lastMass = 0;
    public float Rate { get; private set; } = 0; // in grams / second
    public float Mass { get; set; } // in grams

    private void FixedUpdate() {
        Rate = (Mass - lastMass) / Time.deltaTime;
        lastMass = Mass;
    }
}