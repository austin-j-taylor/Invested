using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Entities controlled by the Wraith that, when defeated, are calmed.
/// </summary>
public class Pacifiable : Entity {

    /// <summary>
    /// returns a RaycastHit object if this entity has line of sight with the target's center of mass.
    /// </summary>
    /// <param name="eyes">the source of sight</param>
    /// <param name="target">the target to look for</param>
    /// <param name="hit">the raycast hit for the line-of-sight check</param>
    /// <returns>a RaycastHit if successfull, null if not.</returns>
    protected bool CanSee(Transform eyes, Rigidbody target, out RaycastHit hit) {
        if (Physics.Raycast(eyes.position, (target.transform.position + target.centerOfMass - eyes.position), out hit)) {
            if (hit.rigidbody != null && hit.rigidbody == target) {
                return true;
            }
        }
        return false;
    }
}
