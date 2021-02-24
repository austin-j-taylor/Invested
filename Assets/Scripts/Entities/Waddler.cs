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
    private const float aim_spherecastRadius = 1;
    #endregion

    public enum WaddlerState { Idle, Suprised, GettingBlock, PickingUp, Caught, MovingToEnemy, AnchoredPull, AnchoredPush, Throwing, Thrown, Pacified }
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

    private Crate targetBlock = null;
    private Crate TargetBlock {
        get => targetBlock;
        set {
            if (targetBlock != null) {
                if (grabJoint != null) {
                    Destroy(grabJoint);
                    targetBlock.Rb.isKinematic = false;
                    foreach (Collider col1 in collidersToIgnore)
                        foreach (Collider col2 in targetBlock.Colliders)
                            Physics.IgnoreCollision(col1, col2, false);

                }
                targetBlock.IsATarget = false;
            }
            targetBlock = value;
            if (value != null) {
                value.IsATarget = true;
            }
        }
    }
    private Actor enemy;

    private float timeInSearch = 0, timeInPickup = 0;

    private void Awake() {
        allomancer = GetComponentInChildren<NonPlayerPushPullController>();
        waddlerAnimation = GetComponentInChildren<WaddlerAnimation>();
        agent = GetComponentInChildren<NavMeshAgent>();
        grabberConstraintSource = new ConstraintSource {
            sourceTransform = grabber,
            weight = 1
        };
        allomancer.CustomCenterOfAllomancy = grabber;
        allomancer.BaseStrength = 3;

        Hostile = true;
        MaxHealth = 2;
        EntityName = "Walkydigit";
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
                if (isDead)
                    State_toPacified();
                else {
                    enemy = Player.CurrentActor;

                    // Wait until the player is in range and visible
                    if (IMath.TaxiDistance(Player.CurrentActor.transform.position, transform.position) < radius_perception) {
                        if (CanSee(eyes, enemy.Rb, out hit)) {
                            State_toSurprised();
                        }
                    }
                }
                break;
            case WaddlerState.Suprised:
                if (isDead)
                    State_toPacified();
                // Transition called by animation
                break;
            case WaddlerState.GettingBlock:
                if (isDead)
                    State_toPacified();
                else {
                    // If we have a target
                    if (TargetBlock != null) {
                        // Start picking up when the block's very close
                        Vector3 diff = transform.position - TargetBlock.transform.position;
                        diff.y = 0;
                        if (diff.magnitude < radius_stopMovingStartPickup) {
                            State_toPickingUp();
                        }
                    }
                }
                break;
            case WaddlerState.PickingUp:
                if (isDead)
                    State_toPacified();
                else {
                    // Stop moving, and start trying to pick up the block
                    // Wait for animation to call "Callback_PickingUp" to start pulling on the cube
                    // Wait for physics to call "OnCollisionEnter" to transition
                    // If the block gets knocked out of "range" or too much time has passed, give up.
                    if (timeInPickup >= timeinPickupMax || Vector3.Distance(transform.position, TargetBlock.transform.position) > radius_stopPickup) {
                        State_toGettingBlock();
                    } else {
                        timeInPickup += Time.deltaTime;
                    }
                }
                break;
            case WaddlerState.Caught:
                if (isDead)
                    State_toPacified();
                // Transition in Callback_Caught
                break;
            case WaddlerState.MovingToEnemy:
                if (isDead)
                    State_toPacified();
                else {
                    // The block is picked up. Move towards the enemy (the player).
                    // If the player starts Pushing or Pulling on the cube, the waddler stops moving and anchors itself.
                    if (TargetBlock.IsBeingPushPulled && enemy.ActorIronSteel.IsPullingOnTarget(TargetBlock)) {
                        State_toAnchoredPull();
                    } else if (TargetBlock.IsBeingPushPulled && enemy.ActorIronSteel.IsPushingOnTarget(TargetBlock)) {
                        State_toAnchoredPush();
                    } else {
                        // Start to throw the block at the enemy, if they are in range and in line of sight.
                        if (Vector3.Distance(transform.position, enemy.transform.position) < radius_throwAtEnemy
                            && CanSeeSpherecast(eyes, enemy.Rb, out hit, aim_spherecastRadius)) {
                            State_toThrowing();
                        }
                    }
                }
                break;
            case WaddlerState.AnchoredPull:
                if (isDead)
                    State_toPacified();
                else {
                    // Wait for player to stop pulling on the block
                    if (!(TargetBlock.IsBeingPushPulled && enemy.ActorIronSteel.IsPullingOnTarget(TargetBlock))) {
                        State_toMovingToEnemy();
                    }
                }
                break;
            case WaddlerState.AnchoredPush:
                if (isDead)
                    State_toPacified();
                else {
                    // Wait for player to stop pushing on the block
                    if (!(TargetBlock.IsBeingPushPulled && enemy.ActorIronSteel.IsPushingOnTarget(TargetBlock))) {
                        State_toMovingToEnemy();
                    }
                }
                break;
            case WaddlerState.Throwing:
                if (isDead)
                    State_toPacified();
                // Transition occurs in "Callback_Throwing"
                break;
            case WaddlerState.Thrown:
                if (isDead)
                    State_toPacified();
                // Transition occurs in "Callback_Thrown"
                break;
            case WaddlerState.Pacified:
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

                    Magnetic oldTargetBlock = TargetBlock;
                    float oldRemainingDistance = TargetBlock == null ? float.PositiveInfinity : IMath.TaxiDistance(transform.position, TargetBlock.transform.position);
                    Collider[] possibleBlocks = Physics.OverlapSphere(transform.position, radius_perception);

                    //Debug.Log("Performing block search. Old target: " + oldTargetBlock + ", distance: " + oldRemainingDistance);

                    // Check those colliders. If any of them are on the "can be picked up by a Waddler" layer, add them for consideration.
                    for (int i = 0; i < possibleBlocks.Length; i++) {
                        if (possibleBlocks[i].gameObject.layer == GameManager.Layer_PickupableByWaddler) {

                            Crate currentTarget = possibleBlocks[i].GetComponentInParent<Crate>();
                            if (currentTarget != null && !currentTarget.IsATarget) {
                                // If someone else is following this cube, ignore it.
                                // If this is the first block, we've seen, choose it.
                                // If it has a closer path than the current path, and it's in line of sight, also choose it.

                                //Debug.Log("Remaining distance: " + agent.remainingDistance);
                                if (TargetBlock == null ||
                                        IMath.TaxiDistance(transform.position, currentTarget.transform.position) < oldRemainingDistance
                                        && CanSee(eyes, currentTarget.Rb, out hit)) {
                                    TargetBlock = currentTarget;
                                    //Debug.Log("New path to a new cube found: " + IMath.TaxiDistance(transform.position, currentTarget.transform.position) + " < " + oldRemainingDistance, TargetBlock.gameObject);
                                    oldTargetBlock = TargetBlock;
                                    oldRemainingDistance = IMath.TaxiDistance(transform.position, TargetBlock.transform.position);
                                }
                            }
                        }
                    }

                    if (oldTargetBlock != TargetBlock) {
                        Debug.Log("Target changed from " + (oldTargetBlock == null ? "null" : oldTargetBlock.name) + " to " + TargetBlock.name, TargetBlock.gameObject);
                    } else {
                        //Debug.Log("Kept old target.", TargetBlock.gameObject);
                    }
                } else {
                    timeInSearch += Time.deltaTime;
                    // Follow same block as before, if it exists
                }
                if (TargetBlock == null) {
                    Debug.Log("No block found. Waddler won't move.", gameObject);
                    agent.SetDestination(transform.position);
                } else {
                    agent.SetDestination(TargetBlock.transform.position);
                }

                //Debug.Log(agent.isStopped + ", " + agent.destination + ", " + agent.desiredVelocity + ", " + agent.velocity);
                break;
            case WaddlerState.PickingUp:
                // rotate to be besides the block
                direction = TargetBlock.transform.position - transform.position;
                lookRotation = Quaternion.AngleAxis(-90, transform.up) * Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * angularSpeed_pickup);
                break;
            case WaddlerState.Caught:
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
            case WaddlerState.Thrown:
                break;
            case WaddlerState.Pacified:
                break;
        }
    }

    private void FixedUpdate() {
        if (State == WaddlerState.MovingToEnemy || State == WaddlerState.Throwing) {
            // definitely could be improved
            foreach (Collider col1 in collidersToIgnore)
                foreach (Collider col2 in TargetBlock.Colliders)
                    Physics.IgnoreCollision(col1, col2, true);
        }
    }

    private void OnCollisionStay(Collision collision) {
        CatchTarget(collision);
    }
    protected override void OnCollisionEnter(Collision collision) {

        // Check if we are picking up a block
        if (!CatchTarget(collision)) {
            base.OnCollisionEnter(collision);
        }
    }

    public override void OnHit(Vector3 sourceLocation, float damage) {
        base.OnHit(sourceLocation, damage);

        if (!isDead) {
            // Play "hit" animation
            waddlerAnimation.OnHit();
        }
    }

    protected override void Die() {
        if (!isDead) {
            // Play "die" animation. Get knocked down, then get back up, pacified.
            waddlerAnimation.Die();

            base.Die();
        }
    }

    // Catch/pick up a block while colliding with it
    // Return true if a block was grabbed
    private bool CatchTarget(Collision collision) {
        // Check if we are picking up a block
        if (State == WaddlerState.PickingUp && allomancer.IsBurning) {
            if (collision.rigidbody != null) {
                if (collision.gameObject.layer == GameManager.Layer_PickupableByWaddler) {
                    TargetBlock = collision.gameObject.GetComponent<Crate>();

                    // Grab onto the block
                    TargetBlock.Rb.isKinematic = true;
                    foreach (Collider col1 in collidersToIgnore)
                        foreach (Collider col2 in TargetBlock.Colliders)
                            Physics.IgnoreCollision(col1, col2, true);
                    grabJoint = TargetBlock.Rb.gameObject.AddComponent<ParentConstraint>();
                    grabJoint.AddSource(grabberConstraintSource);
                    if (TargetBlock.prop_SO != null) {
                        grabJoint.SetTranslationOffset(0, new Vector3(0, TargetBlock.prop_SO.Grab_pos_y, TargetBlock.prop_SO.Grab_pos_z));
                        grabJoint.SetRotationOffset(0, new Vector3(0, 0, TargetBlock.prop_SO.Grab_rotation_z));
                    }
                    grabJoint.constraintActive = true;

                    State_toCaught();
                    return true;
                }
            }
        }
        return false;
    }


    #region callbacks
    /// <summary>
    /// Called when the surprised animation is done.
    /// </summary>
    public void Callback_Surprised() {
        if (State == WaddlerState.Suprised) {
            State_toGettingBlock();
        }
    }

    /// <summary>
    /// Starts pulling on the block to pick up.
    /// </summary>
    public void Callback_PickingUp() {
        if (State == WaddlerState.PickingUp) {
            allomancer.StartBurning();
            allomancer.IronPulling = true;
            allomancer.AddPullTarget(TargetBlock.GetComponent<Magnetic>());
        }
    }

    /// <summary>
    /// Start moving after the block is fully caught
    /// </summary>
    public void Callback_Caught() {
        if (State == WaddlerState.Caught) {
            State_toMovingToEnemy();
        }
    }

    /// <summary>
    /// Throws the held block.
    /// </summary>
    public void Callback_Throwing() {
        if (State == WaddlerState.Throwing) {
            Crate oldTarget = targetBlock;
            TargetBlock = null;
            oldTarget.Rb.AddForce(throwForce * (oldTarget.CenterOfMass - grabber.position).normalized, ForceMode.VelocityChange);

            State_toThrown();
        }
    }

    /// <summary>
    /// Start moving after the block is fully thrown
    /// </summary>
    public void Callback_Thrown() {
        if (State == WaddlerState.Thrown) {
            State_toGettingBlock();
        }
    }
    #endregion

    #region transitions
    private void State_toIdle() {
        State = WaddlerState.Idle;
        waddlerAnimation.State_toIdle();
    }
    private void State_toSurprised() {

        State = WaddlerState.Suprised;
        waddlerAnimation.State_toSurprised();
    }
    private void State_toGettingBlock() {
        allomancer.StopBurning();
        timeInSearch = timeInSearchMax;
        TargetBlock = null;

        State = WaddlerState.GettingBlock;
        waddlerAnimation.State_toGettingBlock();
    }
    private void State_toPickingUp() {
        agent.SetDestination(transform.position);
        timeInPickup = 0;

        State = WaddlerState.PickingUp;
        waddlerAnimation.State_toPickingUp();
    }
    private void State_toCaught() {
        allomancer.StopBurning();
        State = WaddlerState.Caught;
        waddlerAnimation.State_toCaught();
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
    private void State_toThrown() {

        State = WaddlerState.Thrown;
        waddlerAnimation.State_toThrown();
    }
    private void State_toPacified() {
        Hostile = false;
        allomancer.StopBurning();
        agent.SetDestination(transform.position);
        TargetBlock = null;

        State = WaddlerState.Pacified;
        waddlerAnimation.State_toPacified();
    }
    #endregion
}
