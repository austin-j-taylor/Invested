using UnityEngine;

/*
 * Represents the metal reserve residing in an allomancer's stomach.
 */
public class MetalReserve : MonoBehaviour {

    private const float maxCapacity = 500; // stomach can hold up to .5kg of metal, why not

    public bool IsEndless { get; set; } = false; // If true, this reserve will never run out
    
    private double mass = 0;
    private double lastMass = 0;
    public double Rate { get; private set; } = 0; // in grams / second
    public double Mass {
        get {
            return mass;
        }
        set {
            mass = value;
            if (mass < 0)
                mass = 0;
        }
    } // in grams
    public bool HasMass {
        get {
            return IsEndless || mass > 0;
        }
    }

    private void FixedUpdate() {
        if(IsEndless) {
            Rate = 0;
        } else {
            Rate = (Mass - lastMass) / Time.fixedDeltaTime;
            lastMass = Mass;
        }
    }

    // Updates both mass and lastMass to the newMass, so rate doesn't get confused from the sudden change
    public void SetMass(float newMass) {
        mass = newMass;
        lastMass = newMass;
        Rate = 0;
    }
}