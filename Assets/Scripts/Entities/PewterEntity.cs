using UnityEngine;

/// <summary>
/// Distinguishes an entity that uses Pewter shielding before it takes damage.
/// </summary>
[RequireComponent(typeof(AllomanticPewter))]
public class PewterEntity : Entity {

    private AllomanticPewter pewter;

    protected virtual void Awake() {
        pewter = GetComponent<AllomanticPewter>();
    }

    /*
     * If the allomancer enters a collision with a high velocity,
     * they should take damage (eventually. Now, just show some particle effects.)
     */
    protected override void OnCollisionEnter(Collision collision) {
        // just ignore the player pulling coins into themself
        if (!collision.collider.CompareTag("Coin")) {
            // If this was a hard fall, show a particle effect.
            Vector3 thisNormal = collision.GetContact(0).normal;
            if (Vector3.Project(collision.relativeVelocity, thisNormal).sqrMagnitude > fallDamageSquareSpeedThreshold) {
                pewter.HitSurface(-thisNormal);
                pewter.OnHit(collision.GetContact(0).point - transform.position, collision.impulse.magnitude, true);
            }
        }
    }

    /*
     * When taking damage, use pewter, if available.
     */
    public override void OnHit(Vector3 sourceLocation, float damage) {
        base.OnHit(sourceLocation, pewter.OnHit(sourceLocation, damage));
    }
}
