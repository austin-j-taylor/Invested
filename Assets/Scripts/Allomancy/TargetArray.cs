using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetArray {

    // Blue metal line constants
    private const float blueLineTargetedWidthFactor = .06f;
    private const float lightSaberConstant = 1024;
    private readonly Color targetedRedLine = new Color(1, 0, 1);
    private readonly Color targetedGreenLine = new Color(0, 1, 0);
    private readonly Color targetedBlueLine = new Color(0, 0, 1);


    private Magnetic[] targets;

    public int Size { get; private set; } = 1;
    public int Count { get; private set; } = 0;

    public Magnetic this[int key] {
        get {
            return targets[key];
        }
    }

    public TargetArray(int maxNumberOfTargets) {
        targets = new Magnetic[maxNumberOfTargets];
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
     * If newTarget is already within the array or is null, it is not added.
     */
    public void AddTarget(Magnetic newTarget, AllomanticIronSteel allomancer) {
        if (newTarget != null) {
            newTarget.Allomancer = allomancer;
            int indexOfTarget = GetIndex(newTarget);
            if (indexOfTarget >= 0) {   // Target is already in the array

                if (indexOfTarget < Count - 1) { // Target is not already at the end of the array
                    // MoveDown over old version of the target, and add the new one at the end.
                    MoveDown(indexOfTarget, false);
                    targets[Count] = newTarget;
                    Count++;
                }
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
            }
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

    // Removes all targets from the array
    public void Clear(bool setSizeTo1 = false) {
        for (int i = 0; i < Count; i++) {
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
    public void UpdateBlueLines(bool pullTheme) {
        // Go through targets and update their metal lines
        for (int i = 0; i < Count; i++) {
            targets[i].SetBlueLine(
                Player.PlayerIronSteel.CenterOfMass,
                blueLineTargetedWidthFactor * targets[i].Charge,
                Mathf.Exp(-targets[i].LastNetAllomanticForceOnAllomancer.magnitude / lightSaberConstant),
                // 200IQ Ternary Operator
                (pullTheme) ? 
                    SettingsMenu.settingsData.pullTargetLineColor == 0 ? targetedBlueLine : targetedGreenLine
                :
                    SettingsMenu.settingsData.pushTargetLineColor == 0 ? targetedBlueLine : targetedRedLine
                );
        }
    }

    /*
     * Removes all entries out of range
     */
    public void RemoveAllOutOfRange() {
        for(int i = 0; i < Count; i++) {
            if (SettingsMenu.settingsData.pushControlStyle == 0) {
                if (targets[i].LastAllomanticForce.magnitude < SettingsMenu.settingsData.metalDetectionThreshold && targets[i].LastAllomanticForce != Vector3.zero) { // zero on first frame of pushing
                    RemoveTargetAt(i);
                }
            } else { // If using the Magnitude control style, burn rate does not affect how far range of targets
                if (targets[i].LastMaxPossibleAllomanticForce.magnitude < SettingsMenu.settingsData.metalDetectionThreshold && targets[i].LastMaxPossibleAllomanticForce != Vector3.zero) {
                    RemoveTargetAt(i);
                }
            }
        }
    }
}
