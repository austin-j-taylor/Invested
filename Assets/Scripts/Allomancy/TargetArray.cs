using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Array-based data structure used for storing an Allomancer's Pull targets or Push targets
 * 
 * A "Vacuous" target is a target that is removed as soon as a non-vacuous target is added.
 */
public class TargetArray {

    // Player can push on up to 300 targets at once
    public const int largeArrayCapacity = 300; // Area, Bubble Control Mode
    public const int smallArrayCapacity = 30; // Manual, and for Non-player allomancers

    private Magnetic[] targets;

    private int size = 1;
    public int Size {
        get {
            return size;
        }
        set {
            if (value >= targets.Length)
                value = targets.Length;
            if (value < 0)
                value = 0;
            if (Count > value) {
                ReduceCountTo(value);
            }

            size = value;
        }
    }

    public int Count { get; private set; } = 0;
    public int VacuousCount { get; private set; } = 0; // number of vacuous targets at the front of the array. Any targets after this are non-vacuous.

    public float MaxRange { get; set; } = 0; // 0 if ignored (use SettingsData), negative if infinite, positive if this Allomancer has a custom max range

    public TargetArray(int capacity) {
        targets = new Magnetic[capacity];
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
     * Removes a target by index
     */
    public void RemoveTargetAt(int index) {
        if (index < Count) {
            MoveDown(index);
            // if that was a vacuous target, decrease that count
            if (index < VacuousCount) {
                VacuousCount--;
            }
        }
    }


    /*
     * Removes a target by reference
     * return true if it was successfully removed, false if target was not found
     */
    public bool RemoveTarget(Magnetic target, bool clear = true) {
        for (int i = Count - 1; i >= 0; i--) {
            if (targets[i] == target) { // Magnetic was found, move targets along
                MoveDown(i, clear);
                // if that was a vacuous target, decrease that count
                if (i < VacuousCount) {
                    VacuousCount--;
                }
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
    public bool AddTarget(Magnetic newTarget, bool addingVacuous) {
        if (addingVacuous) {
            //Debug.Log("Adding vacuous target, count was  " + Count + " and vc was " + VacuousCount);
            // adding a vacuous target
            //// All other targets must also be vacuous:
            if (Count != VacuousCount) {
                //Debug.LogError("TargetArray: adding a vacuous target when non-vacuous targets are already present (" + Count + " != " + VacuousCount + ")");
                // This happens naturally when e.g. throwing coins
                Clear();
            }
            VacuousCount++;
        } else {
            // we are adding a non-vacuous target
            // if there are any vacuous targets present, remove them first
            if (VacuousCount > 0) {
                RemoveAllVacuousTargets();
                //Debug.LogWarning("TargetArray: adding a real target when non-vacuous targets are present. Removing all targets.");
            }
        }

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
            } else {
                // Count == Size. Move all elements down, delete the first entry, and add newTarget to the end.
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
     * Removes all targets from the array
     */
    public void Clear() {
        for (int i = 0; i < Count; i++) {
            targets[i].Clear();
            targets[i] = null;
        }
        Count = 0;
        VacuousCount = 0;
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

        tempCount = VacuousCount;
        VacuousCount = other.VacuousCount;
        other.VacuousCount = tempCount;
    }

    /*
     * Removes all entries out of range, using the given burn rate
     */
    public void RemoveAllOutOfRange(float burnRate, AllomanticIronSteel allomancer) {
        if (MaxRange == 0) {
            for (int i = Count - 1; i >= 0; i--) {
                if (SettingsMenu.settingsAllomancy.pushControlStyle == 0 && SettingsMenu.settingsGameplay.controlScheme != JSONSettings_Gameplay.Gamepad) {
                    if (!targets[i].IsInRange(allomancer, burnRate)) {
                        RemoveTargetAt(i);
                    }
                } else { // If using the Magnitude control style (or gamepad), burn rate does not affect the range of targets
                    if (!targets[i].IsInRange(allomancer, 1)) {
                        RemoveTargetAt(i);
                    }
                }
            }
        } else if (MaxRange > 0) {
            for (int i = Count - 1; i >= 0; i--) {
                if ((targets[i].CenterOfMass - allomancer.CenterOfMass).sqrMagnitude > MaxRange * MaxRange) {
                    RemoveTargetAt(i);
                }
            }
        } // else: maxrange < 0, no max range
    }
    /*
     * Removes all entries out of the bubble, using the distance from the allomancer
     * 
     * does CLEAR all removed entries.
     */
    public void RemoveAllOutOfBubble(float radius, AllomanticIronSteel allomancer) {
        float sqrRadius = radius * radius;
        for (int i = Count - 1; i >= 0; i--) {
            if ((targets[i].CenterOfMass - allomancer.CenterOfMass).sqrMagnitude > sqrRadius) {
                RemoveTargetAt(i);
            }
        }
    }

    /*
     * Removes all vacuous targets. Called when adding a non-vacuous target.
     */
    public void RemoveAllVacuousTargets() {
        if (VacuousCount > 0) {
            Clear();
        }
    }
    /// <summary>
    /// Removes all vacuous targets that have the given type
    /// </summary>
    /// <param name="type">the type of target to remove</param>
    public void RemoveAllVacuousTargetsOfType(System.Type type) {
        for (int i = VacuousCount - 1; i >= 0; i--) {
            if (targets[i].GetType() == type)
                RemoveTargetAt(i);
        }
    }

    /*
     * Removes targets at or above the specified index
     * e.g. for size 40 and count 4:
     *      [A] [B] [C] [D] [ ] [ ] .. [ ]
     * ReduceCountTo(2) results in size 40 and count 2 (Size property assigns size)
     *      [A] [B] [ ] [ ] [ ] [ ] .. [ ]
     */
    private void ReduceCountTo(int index) {
        for (int i = index; i < Count; i++) {
            targets[i].Clear();
            targets[i] = null;
        }
        Count = index;
    }

    /*
     * Moves all elements down, covering the element at index, making an empty space at Count
     * Decrements Count.
     * e.g. for size 40 and count 4:
     *      [A] [B] [C] [D] [ ] [ ] .. [ ]
     * MoveDown(1) results in size 40 and count 3
     *      [A] [B] [D] [ ] [ ] [ ] .. [ ]
     */
    private void MoveDown(int index, bool clear = true) {
        if (Count == 0)
            return;
        if (clear)
            targets[index].Clear();

        for (int i = index; i < Count - 1; i++) {
            targets[i] = targets[i + 1];
        }
        Count--;
        targets[Count] = null;
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

    public bool IsFull() {
        return size == Count;
    }

    /*
     * Replaces the contents of newTargets into the array. Clears any targets that are currently in targets, but not in newTargets.
     */
    public void ReplaceContents(List<Magnetic> newTargets, bool addingVacuous) {
        // O(n^2)ish but n is never bigger than 100, so this is good enough
        // 1) insert all of this frame's newTargets.
        // 2) iterate over the combined this frame/last frame's targets.
        //   if any of the elements of that list are not in this frame's targets, 
        //   and are also not a bubble target,
        //               remove and Clear() them.
        // make sure newTargets isn't too big
        if (targets.Length < newTargets.Count) {
            newTargets.RemoveRange(targets.Length, newTargets.Count - targets.Length);
        }

        if (addingVacuous) {
            VacuousCount = newTargets.Count;
        } else {
            // we are adding a non-vacuous target
            // if there are any vacuous targets present, remove them first
            if (VacuousCount > 0) {
                RemoveAllVacuousTargets();
            }
        }

        // 1) remove and clear all targets from last frame that are no longer in the array this frame
        for (int i = Count - 1; i >= 0; i--) {
            if (!newTargets.Contains(targets[i])) {
                MoveDown(i);
            }
        }
        // 2) make the size big enough to fit the new targets
        if (newTargets.Count > size)
            Size = newTargets.Count;
        Count = newTargets.Count;
        // 3) copy contents of newTargets into our targets
        for (int i = 0; i < newTargets.Count && i < targets.Length; i++) {
            targets[i] = newTargets[i];
        }
    }
    /*
     * Removes all targets that are in newTargets from the array.
     */
    public void RemoveTargets(List<Magnetic> newTargets) {
        for (int i = Count - 1; i >= 0; i--) {
            for (int j = newTargets.Count - 1; j >= 0; j--) {
                if (targets[i] == newTargets[j]) {
                    MoveDown(i);
                }
            }
        }
    }
}
