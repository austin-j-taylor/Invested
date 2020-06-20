using UnityEngine;
using System.Collections;

/**
 * These projectiles are destroyed some time after being created.
 * If it's being pushed or pulled, wait until it's not to destroy it.
 */
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
