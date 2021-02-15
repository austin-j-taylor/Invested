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

    /// <summary>
    /// returns a RaycastHit object if this entity has line of sight with the target's center of mass, casting with a sphere.
    /// </summary>
    /// <param name="eyes">the source of sight</param>
    /// <param name="target">the target to look for</param>
    /// <param name="hit">the raycast hit for the line-of-sight check</param>
    /// <param name="radius">the radius of the spherecast</param>
    /// <returns>a RaycastHit if successfull, null if not.</returns>
    protected bool CanSeeSpherecast(Transform eyes, Rigidbody target, out RaycastHit hit, float radius) {
        // Need to make sure there's not a wall immediately in front of the eyes that the spherecast would miss
        if (Physics.Raycast(eyes.position, (target.transform.position + target.centerOfMass - eyes.position), out hit)) {
            if (hit.rigidbody != null && hit.rigidbody == target) {
                // Make sure there's a wide berth of space through which the target can be seen
                if (Physics.SphereCast(eyes.position, radius, (target.transform.position + target.centerOfMass - eyes.position), out hit)) {
                    if (hit.rigidbody != null && hit.rigidbody == target) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
