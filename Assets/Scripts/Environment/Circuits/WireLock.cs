using UnityEngine;
using System.Collections;

/*
 * A "Lock" that, when destroyed, turns off.
 */
public class WireLock : SourceBreakable {

    private Magnetic metal;
    private Renderer mount;

    protected override void Awake() {
        metal = GetComponentInChildren<Magnetic>();
        mount = GetComponentInChildren<Renderer>();
        OnByDefault = true;

        base.Awake();
    }

    private void LateUpdate() {
        if (On) {
            if (isBeingDestroyed) {
                if (!metal.IsBeingPushPulled) {
                    CeaseDestroying();
                }
            } else {
                if (metal.IsBeingPushPulled) {
                    StartDestroying();
                }
            }
        }
    }

    protected override void Break() {
        base.Break();
        metal.enabled = false;
    }
    protected override void Repair() {
        base.Repair();
        metal.enabled = true;
    }
}
