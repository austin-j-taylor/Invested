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
    private float radius_stopPickup = 3;
    [SerializeField]
    private float radius_stopMovingStartPickup = 2;
    [SerializeField]
    private float radius_throwAtEnemy = 15;
    [SerializeField]
    private float radius_perception = 50;
    [SerializeField]
    private float angularSpeed_surprised = 20, angularSpeed_pickup = 10, angularSpeed_throwing = 20;
    [SerializeField]
    private float timeinPickupMax = 5;
    private const float timeInSearchMax = 0.5f;
    private const float throwForce = 30;
    #endregion

    public enum WaddlerState { Idle, Suprised, GettingBlock, PickingUp, MovingToEnemy, AnchoredPull, AnchoredPush, Throwing }
    [SerializeField]
    public WaddlerState State;
    private WaddlerAnimation waddlerAnimation;
    private NonPlayerPushPullController allomancer;
    private NavMeshAgent agent;
    private ParentConstraint grabJoint;
    private ConstraintSource grabberConstraintSource;
    [SerializeField]
    private Transform grabber = null, eyes = null;
    [SerializeField]
    private Collider[] collidersToIgnore = null;

    private Magnetic targetBlock = null;
    private Actor enemy;

    private float timeInSearch = 0, timeInPickup = 0;

    private void Awake() {
        allomancer = GetComponentInChildren<NonPlayerPushPullController>();
        waddlerAnimation = GetComponentInChildren<WaddlerAnimation>();
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

        State = WaddlerState.Idle;
    }

    private void Update() {
        RaycastHit hit;

        // Waddler state machine Transitions
        switch (State) {
            case WaddlerState.Idle:
                enemy = Player.CurrentActor;

                // Wait until the player is in range and visible
                if (IMath.TaxiDistance(Player.CurrentActor.transform.position, transform.position) < radius_perception) {
                    if (CanSee(eyes, enemy.Rb, out hit)) {
                        Debug.Log("Player seen! oh no!", gameObject);
                        State_toSurprised();
                    }
                }
                break;
            case WaddlerState.Suprised:
                // Transition called by animation
                break;
            case WaddlerState.GettingBlock:
                // If we have a target
                if (targetBlock != null) {
                    // Start picking up when the block's very close
                    if (Vector3.Distance(transform.position, targetBlock.transform.position) < radius_stopMovingStartPickup) {
                        State_toPickingUp();
                    }
                }
                break;
            case WaddlerState.PickingUp:
                // Stop moving, and start trying to pick up the block
                // Wait for animation to call "Callback_PickingUp" to start pulling on the cube
                // Wait for physics to call "OnCollisionEnter" to transition
                // If the block gets knocked out of "range" or too much time has passed, give up.
                if (timeInPickup >= timeinPickupMax || Vector3.Distance(transform.position, targetBlock.transform.position) > radius_stopPickup) {
                    State_toGettingBlock();
                } else {
                    timeInPickup += Time.deltaTime;
                }
                break;
            case WaddlerState.MovingToEnemy:
                // The block is picked up. Move towards the enemy (the player).
                // If the player starts Pushing or Pulling on the cube, the waddler stops moving and anchors itself.
                if (targetBlock.IsBeingPushPulled && enemy.ActorIronSteel.IsPullingOnTarget(targetBlock)) {
                    State_toAnchoredPull();
                } else if (targetBlock.IsBeingPushPulled && enemy.ActorIronSteel.IsPushingOnTarget(targetBlock)) {
                    State_toAnchoredPush();
                } else {
                    // Start to throw the block at the enemy, if they are in range and in line of sight.
                    if (Vector3.Distance(transform.position, enemy.transform.position) < radius_throwAtEnemy
                        && CanSee(eyes, enemy.Rb, out hit)) {
                        Debug.Log("Throwing at player!", hit.transform.gameObject);
                        State_toThrowing();
                    }
                }
                break;
            case WaddlerState.AnchoredPull:
                // Wait for player to stop pulling on the block
                if (!(targetBlock.IsBeingPushPulled && enemy.ActorIronSteel.IsPullingOnTarget(targetBlock))) {
                    State_toMovingToEnemy();
                }
                break;
            case WaddlerState.AnchoredPush:
                // Wait for player to stop pushing on the block
                if (!(targetBlock.IsBeingPushPulled && enemy.ActorIronSteel.IsPushingOnTarget(targetBlock))) {
                    State_toMovingToEnemy();
                }
                break;
            case WaddlerState.Throwing:
                // Transition occurs in "Callback_Throwing"
                break;
        }

        // Actions
        switch (State) {
            case WaddlerState.Idle:
                break;
            case WaddlerState.Suprised:
                // turn to the player in shock!
                Vector3 direction = enemy.transform.position - transform.position;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * angularSpeed_surprised);
                break;
            case WaddlerState.GettingBlock:

                // Intermittently search for the best block to go towards
                if (timeInSearch >= timeInSearchMax) {
                    timeInSearch = 0;

                    Magnetic oldTargetBlock = targetBlock;
                    float oldRemainingDistance = targetBlock == null ? float.PositiveInfinity : IMath.TaxiDistance(transform.position, targetBlock.transform.position);
                    Collider[] possibleBlocks = Physics.OverlapSphere(transform.position, radius_perception);

                    //Debug.Log("Performing block search. Old target: " + oldTargetBlock + ", distance: " + oldRemainingDistance);

                    // Check those colliders. If any of them are on the "can be picked up by a Waddler" layer, add them for consideration.
                    for (int i = 0; i < possibleBlocks.Length; i++) {
                        if (possibleBlocks[i].gameObject.layer == GameManager.Layer_PickupableByWaddler) {

                            Magnetic currentTarget = possibleBlocks[i].GetComponentInParent<Magnetic>();
                            if (currentTarget != null) {
                                //Debug.Log("Checking cube", currentTarget.gameObject);
                                // If this is the first block, we've seen, choose it.
                                // If it has a closer path than the current path, and it's in line of sight, also choose it.

                                //Debug.Log("Remaining distance: " + agent.remainingDistance);
                                if(targetBlock == null || 
                                        IMath.TaxiDistance(transform.position, currentTarget.transform.position) < oldRemainingDistance
                                        && CanSee(eyes, currentTarget.Rb, out hit)) {
                                    targetBlock = currentTarget;
                                    //Debug.Log("New path to a new cube found: " + IMath.TaxiDistance(transform.position, currentTarget.transform.position) + " < " + oldRemainingDistance, targetBlock.gameObject);
                                    oldTargetBlock = targetBlock;
                                    oldRemainingDistance = IMath.TaxiDistance(transform.position, targetBlock.transform.position);
                                }
                            }
                        }
                    }

                    if (oldTargetBlock != targetBlock) {
                        Debug.Log("Target changed from " + (oldTargetBlock == null ? "null" : oldTargetBlock.name) + " to " + targetBlock.name, targetBlock.gameObject);
                    } else {
                        //Debug.Log("Kept old target.", targetBlock.gameObject);
                    }
                } else {
                    timeInSearch += Time.deltaTime;
                    // Follow same block as before, if it exists
                }
                if (targetBlock == null) {
                    Debug.Log("No block found. Waddler won't move.", gameObject);
                    agent.SetDestination(transform.position);
                } else {
                    agent.SetDestination(targetBlock.transform.position);
                }

                //Debug.Log(agent.isStopped + ", " + agent.destination + ", " + agent.desiredVelocity + ", " + agent.velocity);
                break;
            case WaddlerState.PickingUp:
                // rotate to be besides the block
                direction = targetBlock.transform.position - transform.position;
                lookRotation = Quaternion.AngleAxis(-90, transform.up) * Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * angularSpeed_pickup);
                break;
            case WaddlerState.MovingToEnemy:
                agent.SetDestination(enemy.transform.position);
                //Debug.Log(agent.isStopped + ", " + agent.destination + ", " + agent.desiredVelocity + ", " + agent.velocity);
                break;
            case WaddlerState.AnchoredPull:
                // Aim at the player
                direction = enemy.transform.position - transform.position;
                lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * angularSpeed_throwing);
                break;
            case WaddlerState.AnchoredPush:
                // Aim at the player
                direction = enemy.transform.position - transform.position;
                lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * angularSpeed_throwing);
                break;
            case WaddlerState.Throwing:
                // Aim at the player
                direction = enemy.transform.position - transform.position;
                lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * angularSpeed_throwing);
                break;
        }
                Debug.Log(agent.isStopped);
    }

    private void FixedUpdate() {
        if (State == WaddlerState.MovingToEnemy || State == WaddlerState.Throwing) {
            // definitely could be improved
            foreach (Collider col1 in collidersToIgnore)
                foreach (Collider col2 in targetBlock.Colliders)
                    Physics.IgnoreCollision(col1, col2, true);
        }
    }

    private void OnCollisionStay(Collision collision) {
        OnCollisionEnter(collision);
    }
    protected override void OnCollisionEnter(Collision collision) {
        base.OnCollisionEnter(collision);

        if (State == WaddlerState.PickingUp && allomancer.IsBurning) {
            if (collision.rigidbody != null) {
                if (collision.gameObject.layer == GameManager.Layer_PickupableByWaddler) {
                    targetBlock = collision.gameObject.GetComponent<Magnetic>();

                    // Grab onto the block
                    Debug.Log("stoppedd  burnign");
                    allomancer.StopBurning();
                    targetBlock.Rb.isKinematic = true;
                    foreach (Collider col1 in collidersToIgnore)
                        foreach (Collider col2 in targetBlock.Colliders)
                            Physics.IgnoreCollision(col1, col2, true);
                    grabJoint = targetBlock.Rb.gameObject.AddComponent<ParentConstraint>();
                    grabJoint.AddSource(grabberConstraintSource);
                    if (targetBlock.prop_SO != null) {
                        grabJoint.SetTranslationOffset(0, new Vector3(0, targetBlock.prop_SO.Grab_pos_y, targetBlock.prop_SO.Grab_pos_z));
                        grabJoint.SetRotationOffset(0, new Vector3(0, 0, targetBlock.prop_SO.Grab_rotation_z));
                    }
                    grabJoint.constraintActive = true;

                    State_toMovingToEnemy();
                }
            }
        }
    }

    /// <summary>
    /// Called when the surprised animation is done.
    /// </summary>
    public void Callback_Surprised() {
        State_toGettingBlock();
    }

    /// <summary>
    /// Starts pulling on the block to pick up.
    /// </summary>
    public void Callback_PickingUp() {
        allomancer.StartBurning();
        Debug.Log("starting  burnign");
        allomancer.IronPulling = true;
        allomancer.AddPullTarget(targetBlock.GetComponent<Magnetic>());
    }

    /// <summary>
    /// Throws the held block.
    /// </summary>
    public void Callback_Throwing() {
        Destroy(grabJoint);
        targetBlock.Rb.isKinematic = false;
        foreach (Collider col1 in collidersToIgnore)
            foreach (Collider col2 in targetBlock.Colliders)
                Physics.IgnoreCollision(col1, col2, false);

        targetBlock.Rb.AddForce(throwForce * (targetBlock.CenterOfMass - grabber.position).normalized, ForceMode.VelocityChange);
        targetBlock = null;


        State_toGettingBlock();
    }

    private void State_toIdle() {
        State = WaddlerState.Idle;
        waddlerAnimation.State_toIdle();
    }
    private void State_toSurprised() {

        State = WaddlerState.Suprised;
        waddlerAnimation.State_toSurprised();
    }
    private void State_toGettingBlock() {
        timeInSearch = timeInSearchMax;
        targetBlock = null;

        State = WaddlerState.GettingBlock;
        waddlerAnimation.State_toGettingBlock();
    }
    private void State_toPickingUp() {
        agent.SetDestination(transform.position);
        timeInPickup = 0;

        State = WaddlerState.PickingUp;
        waddlerAnimation.State_toPickingUp();
    }
    private void State_toMovingToEnemy() {
        agent.SetDestination(enemy.transform.position);

        State = WaddlerState.MovingToEnemy;
        waddlerAnimation.State_toMovingToEnemy();
    }
    private void State_toAnchoredPull() {
        agent.SetDestination(transform.position);

        State = WaddlerState.AnchoredPull;
        waddlerAnimation.State_toAnchoredPull();
    }
    private void State_toAnchoredPush() {
        agent.SetDestination(transform.position);

        State = WaddlerState.AnchoredPush;
        waddlerAnimation.State_toAnchoredPush();
    }
    private void State_toThrowing() {
        agent.SetDestination(transform.position);

        State = WaddlerState.Throwing;
        waddlerAnimation.State_toThrowing();
    }
}
