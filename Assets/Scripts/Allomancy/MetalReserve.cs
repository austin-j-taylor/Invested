using UnityEngine;

/*
 * Represents the metal reserve residing in an allomancer's stomach.
 */
public class MetalReserve : MonoBehaviour {

    private const double fuzzyThresholdFull = 1, // 1 gram
                         fuzzyThresholdChanging = .001f; // 1 mg

    public bool IsEndless { get; set; } = false; // If true, this reserve will never run out
    public bool IsEnabled { get; set; } = true; // If false, this reserve is effectively always empty. Overrides IsEndless.
    public bool IsBurnedOut { get; set; } = false; // if the player burns all their mass, they can't use it till it refills
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
            else if (mass > capacity)
                mass = capacity;
        }
    } // in grams
    private double capacity = 150; // stomach can hold up to this much metal
    public double Capacity {
        get {
            return capacity;
        }
        set {
            capacity = value;
            if (mass > capacity)
                mass = capacity;
        }
    }

    public bool HasMass {
        get {
            Debug.Log(IsEnabled + " " + (IsEnabled && (IsEndless || mass > 0) && !IsBurnedOut));
            return IsEnabled && (IsEndless || mass > 0) && !IsBurnedOut;
        }
    }
    public bool IsChanging {
        get {
            return IsEnabled && (Rate < -fuzzyThresholdChanging || Rate > fuzzyThresholdChanging);
        }
    }

    public bool IsFull {
        get {
            return IsEnabled && (mass == capacity);
        }
    }
    public bool IsFullFuzzy {
        get {
            return IsEnabled && (capacity - mass < fuzzyThresholdFull);
        }
    }

    private void FixedUpdate() {
        if(IsEndless || !IsEnabled) {
            Rate = 0;
        } else {
            Rate = (Mass - lastMass) / Time.fixedDeltaTime;
            lastMass = Mass;

            if (IsBurnedOut && mass >= capacity / 2 ) { // stop being burned out once the reserve refills to half capacity
                IsBurnedOut = false;
            }
        }
    }

    // Updates both mass and lastMass to the newMass, so rate doesn't get confused from the discontinuitous change
    public void SetMass(double newMass) {
        Mass = newMass;
        lastMass = mass;
        Rate = 0;

        if(!HasMass) {
            IsBurnedOut = true;
        }
    }

    // Fill this reserve with the amount volume
    // If noMoreThan is above 0, don't fill more than that amount
    public void Fill(double volume, double noMoreThan = 0) {
        if (noMoreThan > 0 && mass + volume > noMoreThan)
            Mass = noMoreThan;
        else
            Mass = mass + volume;
    }

    // fully refills this reserve
    public void Refill() {
        Mass = capacity;
        IsBurnedOut = false;
    }
}