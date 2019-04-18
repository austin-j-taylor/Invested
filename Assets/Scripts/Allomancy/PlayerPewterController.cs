using UnityEngine;
using System.Collections;

/*
 * The AllomanticPewter specific for the Player.
 * Checks for input to apply sprinting, jumping, and shielding.
 */
public class PlayerPewterController : AllomanticPewter {

    private const float pewterAcceleration = 5.5f;
    private const float pewterRunningSpeed = 12.5f;
    private const float jumpHeight = 300;
    private const float jumpDirectionModifier = 400;
    private const float jumpPewterMagnitude = 500;
    protected const float gramsPewterPerJump = 1f;
    protected const float timePewterPerJump = 1.5f;
    private readonly Vector3 particleSystemPosition = new Vector3(0, -.2f, 0);

    private void Update() {
        if (Keybinds.Sprint() && PewterReserve.HasMass) {
            IsSprinting = true;
        } else {
            IsSprinting = false;
        }
    }

    public override void Clear() {
        PewterReserve.SetMass(100);
        base.Clear();
    }

    public float Sprint(Vector3 movement, bool showParticles) {
        if(showParticles) {
            particleDirection = Quaternion.LookRotation(-movement);
            particleSystem.transform.rotation = particleDirection;
            particleSystem.Play();
        }
        return pewterAcceleration * Mathf.Max(pewterRunningSpeed - Vector3.Project(rb.velocity, movement.normalized).magnitude, 0);
    }

    /*
     * If burning pewter, executes a pewter jump.
     * If not burning, executes a normal jump.
     * Returns the force of the jump.
     * 
     * normal: the normal vector of the surface being jumped off of
     */
    public Vector3 Jump(Vector3 movement, Vector3 normal) {
        if(IsSprinting) {
            Drain(gramsPewterPerJump, timePewterPerJump);

            particleSystem.transform.rotation = particleDirection;
            particleSystem.transform.position = Player.PlayerInstance.transform.position + particleSystemPosition;

            if(movement.sqrMagnitude <= .01f) { // Vertical jump
                movement = Vector3.up;
                // If movement is going INTO the wall, we must be jumping up it
            } else if (Vector3.Dot(normal, movement) < -0.01f) {
                float angle = Vector3.Angle(movement, normal) - 90;
                movement = Quaternion.AngleAxis(angle, Vector3.Cross(Vector3.up, normal)) * movement;
                if (movement.y < 0)
                    movement.y = -movement.y;
            }
            //lastnormal = normal.normalized;

            Vector3 force = normal * jumpHeight + movement * jumpDirectionModifier;
            force = Vector3.ClampMagnitude(force, jumpPewterMagnitude);

            particleDirection = Quaternion.LookRotation(-force);
            particleSystem.transform.rotation = particleDirection;
            particleSystem.Play();

            return force;
        }
        return normal * jumpHeight;
    }
}
