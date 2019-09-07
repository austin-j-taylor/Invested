using UnityEngine;

/*
 * Represents the metal reserve residing in an allomancer's stomach.
 */
public class MetalReserve : MonoBehaviour {

    private const double maxCapacity = 150, // stomach can hold up to this much metal
                         fuzzyThresholdFull = 1, // 1 gram
                         fuzzyThresholdChanging = .100f; // 100 mg

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
            else if (mass > maxCapacity)
                mass = maxCapacity;
        }
    } // in grams
    public bool HasMass {
        get {
            return IsEndless || mass > 0;
        }
    }
    public bool IsDraining {
        get { return Rate < -fuzzyThresholdChanging; }
    }
    private bool thisFrameRestoring = false;
    public bool IsRestoring { get; private set; }
    public bool IsFull {
        get { return mass == maxCapacity; }
    }
    public bool IsFullFuzzy {
        get { return maxCapacity - mass < fuzzyThresholdFull; }
    }
    public bool IsChangingFuzzy {
        get { return Rate < 0 ? (Rate < fuzzyThresholdChanging) : (Rate > fuzzyThresholdChanging); }
    }
    public bool IsInEquilibrium {
        get {
            return IsDraining && !((IsRestoring || thisFrameRestoring) && !IsChangingFuzzy);
        }
    }

    private void FixedUpdate() {
        if(IsEndless) {
            Rate = 0;
        } else {
            Rate = (Mass - lastMass) / Time.fixedDeltaTime;
            lastMass = Mass;
        }
        IsRestoring = thisFrameRestoring;
        thisFrameRestoring = false;
    }

    // Updates both mass and lastMass to the newMass, so rate doesn't get confused from the discontinuitous change
    public void SetMass(double newMass) {
        Mass = newMass;
        lastMass = mass;
        Rate = 0;
    }

    // Fill this reserve with the amount volume
    // If noMoreThan is above 0, don't fill more than that amount
    public void Fill(double volume, double noMoreThan = 0) {
        thisFrameRestoring = true;
        if (noMoreThan > 0 && mass + volume > noMoreThan)
            Mass = noMoreThan;
        else
            Mass = mass + volume;
    }
}