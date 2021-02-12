using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.AI;

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
    private NavMeshAgent agent;
    private ParentConstraint grabJoint;
    private ConstraintSource grabberConstraintSource;
    [SerializeField]
    private Transform grabber = null, eyes = null;
    [SerializeField]
    private Collider[] collidersToIgnore = null;

    private Magnetic target = null;
    private Actor enemy;

    private void Awake() {
        allomancer = GetComponentInChildren<NonPlayerPushPullController>();
        rb = GetComponentInChildren<Rigidbody>();
        agent = GetComponentInChildren<NavMeshAgent>();
        grabberConstraintSource = new ConstraintSource {
            sourceTransform = grabber,
            weight = 1
        };
        allomancer.CustomCenterOfAllomancy = grabber;
        allomancer.BaseStrength = 2;
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
                    if (possibleBlocks[i].gameObject.layer == GameManager.Layer_PickupableByWaddler) {
                        possiblyClosestTarget = possibleBlocks[i].GetComponentInParent<Magnetic>();
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
                    state = WaddlerState.MovingToBlock;
                    agent.isStopped = false;
                    agent.SetDestination(target.transform.position);
                }
                break;
            case WaddlerState.MovingToBlock:
                // Move towards the block
                if(Vector3.Distance(transform.position, target.transform.position) < radius_stopMovingStartPickup) {
                    state = WaddlerState.PickingUp;
                    agent.isStopped = true;
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
                if (Vector3.Distance(transform.position, enemy.transform.position) < radius_throwAtEnemy
                    && Physics.Raycast(eyes.position, (enemy.transform.position + enemy.Rb.centerOfMass - eyes.position), out RaycastHit hit, 1000)) {
                    if(hit.rigidbody != null && hit.rigidbody == enemy.Rb) {
                        agent.isStopped = true;
                        state = WaddlerState.Throwing;
                        anim.SetTrigger("Throw");
                        Debug.Log("Hit!", hit.transform.gameObject);
                    } else {

                    }
                }
                break;
            case WaddlerState.Throwing:
                // Transition occurs in "ThrowTarget".
                break;
        }

        // Actions
        switch (state) {
            case WaddlerState.Idle:
                anim.SetBool("Walking", false);
                anim.SetBool("Grabbed", false);
                break;
            case WaddlerState.MovingToBlock:
                agent.SetDestination(target.transform.position);
                //Debug.Log(agent.isStopped + ", " + agent.destination + ", " + agent.desiredVelocity + ", " + agent.velocity);
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
                agent.SetDestination(enemy.transform.position);
                //Debug.Log(agent.isStopped + ", " + agent.destination + ", " + agent.desiredVelocity + ", " + agent.velocity);
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

    private void OnCollisionStay(Collision collision) {
        OnCollisionEnter(collision);
    }
    protected override void OnCollisionEnter(Collision collision) {
        base.OnCollisionEnter(collision);

        if (state == WaddlerState.Catching) {
            if (collision.rigidbody != null) {
                if (collision.gameObject.layer == GameManager.Layer_PickupableByWaddler) {
                //if (collision.rigidbody == target.Rb) {
                    // Catch the target.
                    allomancer.StopBurning();
                    enemy = Player.CurrentActor;
                    state = WaddlerState.MovingToEnemy;
                    agent.isStopped = false;
                    agent.SetDestination(enemy.transform.position);
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
