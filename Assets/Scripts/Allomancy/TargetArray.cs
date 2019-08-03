using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Array-based data structure used for storing an Allomancer's Pull targets or Push targets
 */
public class TargetArray {
    
    public const int arraySize = 30; // Area, Bubble Control Mode

    public const float lightSaberConstant = 1024;
    private const float blueLineTargetedWidthFactor = .06f;
    public static readonly Color targetedRedLine = new Color(1, 0, 1);
    private static readonly Color targetedGreenLine = new Color(0, 1, 0);
    private static readonly Color targetedBlueLine = new Color(0, 0, 1);
    private static readonly Color targetedLightBlueLine = new Color(0, .5f, 1f);

    private Magnetic[] targets;
    private Magnetic vacuousTarget; // When no targets are selected, the vacuous target may still be Pushed on.
    
    public int Size { get; private set; } = 1; // When in Manual, ranges from [1:10]
    public int Count { get; private set; } = 0;

    public float MaxRange { get; set; } = 0; // 0 if ignored (use SettingsData), negative if infinite, positive if this Allomancer has a custom max range
    
    public TargetArray() {
        targets = new Magnetic[arraySize];
    }

    /*
     * Calculates the center of MAGNETIC mass of all targets within this TargetArray.
     */
    public Vector3 CenterOfMagneticMasses() {
        if (Count > 0) {
            Vector3 centerOfMasses = Vector3.zero;
            float mass = 0;
            for (int i = 0; i < Count; i++) {
                centerOfMasses += targets[i].CenterOfMass * targets[i].MagneticMass;
                mass += targets[i].MagneticMass;
            }
            return centerOfMasses / mass;
        } else {
            return Vector3.zero;
        }
    }

    /*
     * The sum of all MAGNETIC mass within this array
     */
    public float NetMagneticMass() {
        float mass = 0;
        for (int i = 0; i < Count; i++) {
            mass += targets[i].MagneticMass;
        }
        return mass;
    }

    /*
     * The charge of the sum of each target's mass
     */
    public float NetCharge() {
        return Mathf.Pow(NetMagneticMass(), AllomanticIronSteel.chargePower);
    }

    /*
     * The sum of each target's charge
     */
    public float SumOfCharges() {
        float charge = 0;
        for (int i = 0; i < Count; i++) {
            charge += targets[i].Charge;
        }
        return charge;
    }

    public Magnetic this[int key] {
        get {
            return targets[key];
        }
    }

    /*
     * Moves all elements down, covering the element at index, making an empty space at Count
     * Decrements Count.
     */
    private void MoveDown(int index, bool clear = true) {
        if(clear)
            targets[index].Clear();

        for (int i = index; i < Count - 1; i++) {
            targets[i] = targets[i + 1];
        }
        Count--;
        targets[Count] = null;
    }

    /*
     * Removes a target by index
     */
    public void RemoveTargetAt(int index) {
        if (index < Count) {
            MoveDown(index);
        }
    }

    /*
     * Removes a target by reference
     * return true if it was successfully removed, false if target was not found
     */
    public bool RemoveTarget(Magnetic target, bool clear = true) {
        for (int i = 0; i < Count; i++) {
            if (targets[i] == target) { // Magnetic was found, move targets along
                MoveDown(i, clear);
                return true;
            }
        }
        return false;
    }

    /*
     * Addes newTarget to the array.
     *      newTarget is added to the first empty space in the array.
     *      If there are no empty spaces, the oldest entry (targets[0]) is overritten by MoveDown(0)
     *      and newTarget is added at the ent of the array.
     * If newTarget is already within the array, it is moved to the front.
     * Returns true if newTarget was not already within the array and false if it was already in the array.
     */
    public bool AddTarget(Magnetic newTarget, AllomanticIronSteel allomancer) {
        int indexOfTarget = GetIndex(newTarget);
        if (indexOfTarget >= 0) {   // Target is already in the array

            if (indexOfTarget < Count - 1) { // Target is not already at the end of the array
                // MoveDown over old version of the target, and add the new one at the end.
                MoveDown(indexOfTarget, false);
                targets[Count] = newTarget;
                Count++;
            }
            return false;
        } else { // Target is not already in the array.
            // If Count < Size, just add newTarget at Count and increment Count.
            if (Count < Size) {
                targets[Count] = newTarget;
                Count++;
            } else {    // Count == Size. Move all elements down, delete the first entry, and add newTarget to the end.
                        // Do not increment Count, since the number of entries doesn't change.
                MoveDown(0);
                targets[Count] = newTarget;
                Count++;
            }
            return true;
        }
    }

    /*
     * Returns true if potentialTarget is in the array, false otherwise
     */
    public bool IsTarget(Magnetic potentialTarget) {
        for (int i = 0; i < Count; i++) {
            if (potentialTarget == targets[i])
                return true;
        }
        return false;
    }

    /*
     * Returns the index of potentialTarget in targets.
     * Returns -1 if potentialTarget is not in targets.
     */
    private int GetIndex(Magnetic potentialTarget) {
        for (int i = 0; i < Count; i++) {
            if (potentialTarget == targets[i])
                return i;
        }
        return -1;
    }

    /*
     * Removes all targets from the array
     */
    public void Clear(bool setSizeTo1 = false, bool clearTargets = true) {
        for (int i = 0; i < Count; i++) {
            if(clearTargets)
                targets[i].Clear();
            targets[i] = null;
        }
        Count = 0;
        if (setSizeTo1)
            Size = 1;
    }

    /*
     * Increments the available number of targets in the array.
     */
    public void IncrementSize() {
        if (Size < targets.Length) {
            Size++;
        }
    }

    /*
     * Decrements the available number of targets in the array.
     * If there are too many targets after decrementing the Size, remove the oldest one.
     * Cannot go below 1.
     */
    public void DecrementSize() {
        if (Size > 1) {
            Size--;
            if (Count > Size) {
                RemoveTargetAt(0);
            }
        }
    }

    /*
     * Swaps the contents of this TargetArray with another.
     * They should have the same Size.
     */
    public void SwapContents(TargetArray other) {
        Magnetic[] temp = targets;
        targets = other.targets;
        other.targets = temp;

        int tempCount = Count;
        Count = other.Count;
        other.Count = tempCount;
    }
    /*
     * Refreshes the blue metal lies that point to each target.
     * pullTheme determines the color (green or red) that the line could have.
     */
    public void UpdateBlueLines(bool pullingColor, float burnRate, Vector3 startPos) {
        // Go through targets and update their metal lines
        for (int i = 0; i < Count; i++) {
            targets[i].SetBlueLine(
                startPos,
                blueLineTargetedWidthFactor * targets[i].Charge,
                Mathf.Exp(-targets[i].LastMaxPossibleAllomanticForce.magnitude * burnRate / lightSaberConstant),
                // 200IQ Ternary Operator
                (pullingColor) ?
                    SettingsMenu.settingsData.pullTargetLineColor == 0 ? targetedBlueLine
                    :
                        SettingsMenu.settingsData.pullTargetLineColor == 1 ? targetedLightBlueLine
                        :
                        targetedGreenLine
                :
                    SettingsMenu.settingsData.pushTargetLineColor == 0 ? targetedBlueLine : targetedRedLine
                );

        }
    }

    /*
     * Removes all entries out of range, using the given burn rate
     */
    public void RemoveAllOutOfRange(float burnRate, AllomanticIronSteel allomancer) {
        if(MaxRange == 0) {
            for (int i = 0; i < Count; i++) {
                if (SettingsMenu.settingsData.pushControlStyle == 0 && SettingsMenu.settingsData.controlScheme != SettingsData.Gamepad) {
                    if (!targets[i].IsInRange(allomancer, burnRate)) {
                        RemoveTargetAt(i);
                    }
                } else { // If using the Magnitude control style (or gamepad), burn rate does not affect the range of targets
                    if (!targets[i].IsInRange(allomancer, 1)) {
                        RemoveTargetAt(i);
                    }
                }
            }
        } else if(MaxRange > 0) {
            for (int i = 0; i < Count; i++) {
                if ((targets[i].CenterOfMass - allomancer.CenterOfMass).sqrMagnitude > MaxRange * MaxRange) {
                    RemoveTargetAt(i);
                }
            }
        } // else: maxrange < 0, no max range
    }
}
