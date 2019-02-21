using UnityEngine;

/*
 * Distinguishes an entity that uses Pewter shielding before it takes damage.
 */
[RequireComponent(typeof(AllomanticPewterController))]
public class PewterEntity : Entity {
    
    /*
     * When taking damage, use pewter, if available.
     */
    public override void OnHit(float damage) {
        base.OnHit(damage);
    }
}
