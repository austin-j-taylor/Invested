using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

/// <summary>
/// A 3-legged creature that picks up blocks and throws them at the player.
/// </summary>
public class Waddler : Pacifiable {

    #region
    [SerializeField]
    private float radius_loseTarget = 3;
    [SerializeField]
    private float radius_stopMovingStartPickup = 2;
    [SerializeField]
    private float radius_throwAtEnemy = 15;
    private const float timeInPickingUpMax = 2;
    private const float perceptRange = 50;
    private const float throwForce = 30;
    #endregion

    private enum WaddlerState { Idle, MovingToBlock, PickingUp, Catching, MovingToEnemy, Throwing }

    [SerializeField]
    private WaddlerState state;
    private NonPlayerPushPullController allomancer;
    private ParentConstraint grabJoint;
    private ConstraintSource grabberConstraintSource;
    [SerializeField]
    private Transform grabber = null;
    [SerializeField]
    private Collider[] collidersToIgnore = null;

    private Magnetic target = null;
    private Actor enemy;

    private void Awake() {
        allomancer = GetComponentInChildren<NonPlayerPushPullController>();
        rb = GetComponentInChildren<Rigidbody>();
        grabberConstraintSource = new ConstraintSource {
            sourceTransform = grabber,
            weight = 1
        };
        allomancer.CustomCenterOfAllomancy = grabber;
    }

    protected override void Start() {
        base.Start();

        state = WaddlerState.Idle;
    }

    private void Update() {
        // Transitions
        switch (state) {
            case WaddlerState.Idle:
                // Chose a block and move towards it.
                Collider[] possibleBlocks = Physics.OverlapSphere(transform.position, perceptRange);
                Magnetic possiblyClosestTarget = null;
                float closestDistance = radius_loseTarget + 10;
                // Check those colliders. If any of them have CanBePickedUpByWaddler, pick it up.
                for (int i = 0; i < possibleBlocks.Length; i++) {
                    CanBePickedUpByWaddler canBePickedUp = possibleBlocks[i].GetComponent<CanBePickedUpByWaddler>();
                    if (canBePickedUp != null) {
                        possiblyClosestTarget = canBePickedUp.GetComponentInParent<Magnetic>();
                        float possiblyClosestDistance = Vector3.Distance(transform.position, possiblyClosestTarget.transform.position);
                        if (target == null || possiblyClosestDistance < closestDistance) {
                            target = possiblyClosestTarget;
                            closestDistance = possiblyClosestDistance;
                        }
                    }
                }
                if (target == null) {
                    Debug.Log("No target found");
                } else {
                    Debug.Log("Found block: ", target.gameObject);
                    state = WaddlerState.MovingToBlock;
                }
                break;
            case WaddlerState.MovingToBlock:
                // Move towards the block
                if(Vector3.Distance(transform.position, target.transform.position) < radius_stopMovingStartPickup) {
                    state = WaddlerState.PickingUp;
                    anim.SetTrigger("PickingUp");
                }
                break;
            case WaddlerState.PickingUp:
                // Stop moving, and start trying to pick up the block
                // Wait for animation to call "PullOnTarget", when the transition happens
                break;
            case WaddlerState.Catching:
                // Be pulling on the block.
                // If the block gets knocked out of "range" or too much time has passed, give up.
                // Transition happens in OnCollisionEnter.
                break;
            case WaddlerState.MovingToEnemy:
                // The block is picked up. Move towards the enemy (the player).
                // Start to throw the block at the enemy, if they are in range and in line of sight.
                if (Vector3.Distance(transform.position, enemy.transform.position) < radius_throwAtEnemy) {
                    state = WaddlerState.Throwing;
                }
                break;
            case WaddlerState.Throwing:
                // Transition occurs in "ThrowTarget".
                anim.SetTrigger("Throw");
                break;
        }

        // Actions
        switch (state) {
            case WaddlerState.Idle:
                anim.SetBool("Walking", false);
                anim.SetBool("Grabbed", false);
                break;
            case WaddlerState.MovingToBlock:
                anim.SetBool("Walking", true);
                anim.SetBool("Grabbed", false);
                break;
            case WaddlerState.PickingUp:
                anim.SetBool("Walking", false);
                anim.SetBool("Grabbed", false);
                break;
            case WaddlerState.Catching:
                anim.SetBool("Walking", false);
                anim.SetBool("Grabbed", false);
                break;
            case WaddlerState.MovingToEnemy:
                anim.SetBool("Walking", true);
                anim.SetBool("Grabbed", true);
                break;
            case WaddlerState.Throwing:
                anim.SetBool("Walking", false);
                anim.SetBool("Grabbed", true);
                break;
        }
    }

    private void FixedUpdate() {
        if (state == WaddlerState.MovingToEnemy || state == WaddlerState.Throwing) {
            // definitely could be improved
            foreach (Collider col1 in collidersToIgnore)
                foreach (Collider col2 in target.Colliders)
                    Physics.IgnoreCollision(col1, col2, true);
        }
    }

    protected override void OnCollisionEnter(Collision collision) {
        base.OnCollisionEnter(collision);

        if (state == WaddlerState.Catching) {
            if (collision.rigidbody != null) {
                if (collision.rigidbody == target.Rb) {
                    // Catch the target.
                    allomancer.StopBurning();
                    enemy = Player.CurrentActor;
                    state = WaddlerState.MovingToEnemy;
                    anim.SetTrigger("Caught");

                    target.Rb.isKinematic = true;
                    foreach (Collider col1 in collidersToIgnore)
                        foreach (Collider col2 in target.Colliders)
                            Physics.IgnoreCollision(col1, col2, true);
                    grabJoint = target.Rb.gameObject.AddComponent<ParentConstraint>();
                    grabJoint.AddSource(grabberConstraintSource);
                    if (target.prop_SO != null) {
                        grabJoint.SetTranslationOffset(0, new Vector3(0, target.prop_SO.Grab_pos_y, target.prop_SO.Grab_pos_z));
                        grabJoint.SetRotationOffset(0, new Vector3(0, 0, target.prop_SO.Grab_rotation_z));
                    }
                    grabJoint.constraintActive = true;
                }
            }
        }
    }

    /// <summary>
    /// Starts pulling on the target to pick up.
    /// </summary>
    public void PullOnTarget() {
        allomancer.StartBurning();
        allomancer.IronPulling = true;
        allomancer.AddPullTarget(target);

        state = WaddlerState.Catching;
    }
    
    /// <summary>
    /// Throws the held block.
    /// </summary>
    public void ThrowTarget() {
        Destroy(grabJoint);
        target.Rb.isKinematic = false;
        foreach (Collider col1 in collidersToIgnore)
            foreach (Collider col2 in target.Colliders)
                Physics.IgnoreCollision(col1, col2, false);

        target.Rb.AddForce(throwForce * (target.CenterOfMass - grabber.position).normalized, ForceMode.VelocityChange);

        target = null;

        state = WaddlerState.Idle;
    }
}
