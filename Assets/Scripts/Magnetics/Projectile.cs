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
    }

    IEnumerator WaitThenDestroy() {
        yield return new WaitForSeconds(lifetime);
        while (IsBeingPushPulled)
            yield return null;
        Destroy(gameObject);
    }
}
