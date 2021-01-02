using UnityEngine;
using System.Collections;

/// <summary>
/// Represents a projectile like a bullet that vanishes some time after being spawned.
/// </summary>
public class Projectile : Magnetic {

    private const float lifetime = 10;

    protected override void Start() {
        base.Start();
        StartCoroutine(WaitThenDestroy());
        StartCoroutine(WaitThenApplyGravity());
    }

    IEnumerator WaitThenDestroy() {
        yield return new WaitForSeconds(lifetime);
        while (IsBeingPushPulled)
            yield return null;
        Destroy(gameObject);
    }
    IEnumerator WaitThenApplyGravity() {
        // Hacky, but make gravity be weaker on the projectile for a short time after it's been fired
        // to make the shot be more accurate.
        // Once the turrets have a more accurate firing calculation, this'll go away.
        yield return null;
        while (IsBeingPushPulled) {
            Rb.AddForce(-Physics.gravity, ForceMode.Acceleration);
            yield return null;
        }
    }
}
