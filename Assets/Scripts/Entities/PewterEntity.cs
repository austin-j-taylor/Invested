using UnityEngine;

/*
 * Distinguishes an entity that uses Pewter shielding before it takes damage.
 */
[RequireComponent(typeof(AllomanticPewter))]
public class PewterEntity : Entity {

    private AllomanticPewter pewter;

    protected virtual void Awake() {
        pewter = GetComponent<AllomanticPewter>();
    }

    /*
     * When taking damage, use pewter, if available.
     */
    public override void OnHit(float damage) {
        base.OnHit(pewter.OnHit(damage));
    }
}
