using UnityEngine;
using System.Collections;

/*
 * The AllomanticPewter specific for the Player.
 * Checks for input to apply sprinting, jumping, and shielding.
 */
public class PlayerPewterController : AllomanticPewter {

    private const float pewterAcceleration = 5.5f;
    private const float pewterRunningSpeed = 12.5f;
    private const float jumpHeight = 400;
    private const float jumpPewterMagnitude = 600;
    private const float jumpDirectionModifier = 350;
    protected const float gramsPewterPerJump = 3f;
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

    public float Sprint(Vector3 movement, bool lastWasSprinting) {
        if (IsSprinting) {
            if(!lastWasSprinting) {
                particleDirection = Quaternion.LookRotation(-movement);
                particleSystem.transform.rotation = particleDirection;
                particleSystem.Play();
            }
            return pewterAcceleration * Mathf.Max(pewterRunningSpeed - Vector3.Project(rb.velocity, movement.normalized).magnitude, 0);
        }
        return 0;
    }

    /*
     * If burning pewter, executes a pewter jump.
     * If not burning, executes a normal jump.
     * Returns the force of the jump.
     */
    public Vector3 Jump(Vector3 movement, Vector3 force) {
        if(IsSprinting) {
            Drain(gramsPewterPerJump, timePewterPerJump);

            particleSystem.transform.rotation = particleDirection;
            particleSystem.transform.position = Player.PlayerInstance.transform.position + particleSystemPosition;

            if(movement.sqrMagnitude <= .01f) { // Vertical jump
                movement = Vector3.up;

            } else if (Vector3.Dot(force, movement) < -0.01f) {
                float angle = Vector3.Angle(movement, force);
                angle -= 90;
                movement = Quaternion.AngleAxis(angle, Vector3.Cross(Vector3.up, force)) * movement;
                if (movement.y < 0)
                    movement.y = -movement.y;
            }
            //lastForce = force.normalized;

            force = force * jumpHeight + movement * jumpDirectionModifier;
            Vector3.ClampMagnitude(force, jumpPewterMagnitude);

            particleDirection = Quaternion.LookRotation(-force);
            particleSystem.transform.rotation = particleDirection;
            particleSystem.Play();
        } else {
            force *= jumpHeight;
        }

        return force;
    }
}
