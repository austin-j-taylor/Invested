using UnityEngine;
using System.Collections;

/**
 * These projectiles are destroyed some time after being created.
 */
public class Projectile : MonoBehaviour {

    private const float lifetime = 10;

    IEnumerator Start() {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}
