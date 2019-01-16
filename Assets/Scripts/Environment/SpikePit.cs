using UnityEngine;
using System.Collections;

public class SpikePit : MonoBehaviour {

    private const float slerpConstantPlayer = 8f;
    private const float slerpConstantPath = 8f;
    private const float slerpTimeChargeupPlayer = .5f;
    private const float slerpTimeChargeupPathRotation = .3f;
    private const float slerpTimeChargeupPathReturn = 1f;
    private const float slerpTimeChargeupPath = 2f;
    private const float anglePullThreshold = 45f;
    private const float angleEqualThreshold = 1f;
    private const float distanceThresholdSpiking = 1.9f;
    private const float distanceThresholdEqual = .15f;
    private const float distanceThresholdReturn = 5;
    private const float forwardTimeOffset = .2f;
    private const float forceConstantFar = 200f;
    private const float dragTracing = 5;
    private const float dragChasing = .3f;
    private const float animationTime = 4;

    private bool facingPlayer;
    private bool tracingPlayer;
    private bool chasingPlayer;
    private bool spikingPlayer;
    private bool tracingPath;
    private bool releasingPlayer;
    private bool returningHome;
    private float slerpTime;
    private float slerpTimeRotation;
    private float progress;
    private float startTime;

    private Animator anim;
    private Transform spikeTarget;
    private Rigidbody spikeRb;
    private NonPlayerPushPullController spike;
    private SpikeSpline splineDragging;
    private SpikeSpline splineReturnHome;

    void Start() {
        anim = GetComponent<Animator>();
        spike = GetComponentInChildren<NonPlayerPushPullController>();
        spikeRb = spike.GetComponent<Rigidbody>();
        spikeTarget = spike.transform.parent;
        SpikeSpline[] splines = GetComponentsInChildren<SpikeSpline>();
        splineDragging = splines[0];
        splineReturnHome = splines[1];

        spike.LinesAreVisibleWhenNotBurning = true;

        facingPlayer = false;
        tracingPlayer = false;
        chasingPlayer = false;
        spikingPlayer = false;
        tracingPath = false;
        releasingPlayer = false;
        returningHome = false;
        startTime = 0;
        slerpTime = 0;
        slerpTimeRotation = 0;
        progress = 0;
    }

    private void Update() {
        if (!PauseMenu.IsPaused) {
            //Debug.Log(facingPlayer + " . " + tracingPlayer + " > " + chasingPlayer + " > " + tracingPath + " > " + releasingPlayer);

            // Executes during Rising. Face the Spike towards the player, vertically.
            if (facingPlayer) {
                // Rotate Target towards Player
                Vector3 distancetoPlayer = spike.CenterOfMass- Player.PlayerIronSteel.CenterOfMass;
                float angle = 180 - Mathf.Atan2(distancetoPlayer.x, distancetoPlayer.z) * Mathf.Rad2Deg;

                //Vector3 newRotation = Quaternion.LookRotation(Player.PlayerIronSteel.CenterOfMass - spikeTarget.position).eulerAngles;
                //Vector3 newRotation = Quaternion.Slerp(spikeTarget.rotation, Quaternion.LookRotation(Player.PlayerIronSteel.CenterOfMass - spikeTarget.position), slerpConstantPlayer * Time.deltaTime).eulerAngles;
                Vector3 newRotation;
                newRotation.z = angle;
                newRotation.x = 90;
                newRotation.y = 0;
                //spikeTarget.eulerAngles = newRotation;
                spike.transform.eulerAngles = Quaternion.Slerp(spike.transform.rotation, Quaternion.Euler(newRotation), slerpConstantPlayer * Time.deltaTime).eulerAngles;


            }

            // Executes after Rising. The spike angles itself with the spike towards the player. Once the angle is somewhat low, it begins Pulling on the player. 
            if (tracingPlayer) {
                Quaternion newRotation = Quaternion.Slerp(spike.transform.rotation, Quaternion.LookRotation(Player.PlayerIronSteel.CenterOfMass - spike.CenterOfMass), slerpConstantPlayer * Time.deltaTime * slerpTime);
                slerpTime = slerpTimeChargeupPlayer * (Mathf.Exp(-Time.time + startTime) + Time.time - startTime + 1);
                spike.transform.rotation = newRotation;
                //spike.transform.position = spikeTarget.transform.position;

                float angle = Quaternion.Angle(Quaternion.LookRotation(Player.PlayerIronSteel.CenterOfMass - spike.CenterOfMass), newRotation);
                if (angle < anglePullThreshold && spike.PullTargets.Count == 0) {
                    // add player as a pull target
                    spike.AddPullTarget(Player.PlayerMagnetic);
                    spike.IronPulling = true;
                    spike.IronBurnRateTarget = .2f;
                    spike.PullTargets.MaxRange = -1;
                }
                if (angle < angleEqualThreshold) {
                    tracingPlayer = false;
                    chasingPlayer = true;
                    slerpTime = 0;


                    spikeRb.isKinematic = false;
                    spikeRb.drag = dragChasing;
                    spikeRb.velocity = Vector3.zero;
                    spikeRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    spike.IronBurnRateTarget = .75f;
                }
            }

            // The Spike begins chasing the player as it Pulls on the player.
            if (chasingPlayer) {

                Quaternion newRotation = Quaternion.LookRotation(Player.PlayerIronSteel.CenterOfMass - spike.CenterOfMass);
                spike.transform.rotation = newRotation;

                if ((Player.PlayerIronSteel.CenterOfMass - spike.CenterOfMass).magnitude < distanceThresholdSpiking) {
                    chasingPlayer = false;
                    spikingPlayer = true;

                    Player.CanControlPlayer = false;
                    Player.PlayerInstance.GetComponent<Rigidbody>().velocity = spikeRb.velocity;
                }
            }

            // The spike has touched the player. Freeze the player and keep pulling the spike into them.
            if(spikingPlayer) {

                //Quaternion newRotation = Quaternion.LookRotation(Player.PlayerIronSteel.CenterOfMass - spike.CenterOfMass);
                //spike.transform.rotation = newRotation;

                if ((Player.PlayerIronSteel.CenterOfMass - spike.CenterOfMass).magnitude < distanceThresholdEqual) {
                    spikingPlayer = false;
                    tracingPath = true;

                    spikeRb.drag = dragTracing;

                    // Set Player constraint
                    Player.PlayerInstance.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

                    // Make Spike Target rotate towards spline velocity
                    spikeTarget.transform.position = spike.transform.position;
                    spike.transform.localPosition = Vector3.zero;
                    //spikeTarget.transform.rotation = spike.transform.rotation;
                    //spike.transform.localRotation = Quaternion.identity;
                    Player.PlayerInstance.transform.SetParent(spike.transform);
                    Player.PlayerInstance.transform.localPosition = Vector3.zero;

                    splineDragging.SetInitialPoint(transform.InverseTransformPoint(spike.transform.position), spikeRb.velocity.normalized);

                    spike.IronBurnRateTarget = 0;
                    spike.IronPulling = false;
                }
            }

            // Follow the Path.
            if (tracingPath) {
                progress += Time.deltaTime / animationTime * slerpTime;
                slerpTime = Mathf.Min(1, slerpTime + Time.deltaTime * slerpTimeChargeupPath);
                if (progress < 1f) {
                    splineDragging.FollowCurve(spikeTarget.transform, progress, false);

                    // If the spike was pulled away from its path, force the spike back towards the path
                    Vector3 distance = spikeTarget.position - spike.CenterOfMass;
                    float sqrDistance = distance.sqrMagnitude;
                    spikeRb.AddForce(forceConstantFar * distance.normalized * sqrDistance, ForceMode.Acceleration);

                    // Rotate the spike to follow the path, a few moments in the future
                    float offsetTime = progress + forwardTimeOffset / animationTime;
                    offsetTime = offsetTime > 1 ? 1 : offsetTime;
                    //Debug.Log("Distance: " + distance.magnitude);
                    //if (offsetTime < 1) {
                    //Vector3 velocity = splineDragging.GetVelocity(offsetTime);
                    //Quaternion newRotation = Quaternion.LookRotation(splineDragging.GetPoint(offsetTime) - spikeTarget.position);
                    Quaternion newRotation = Quaternion.Slerp(spike.transform.rotation, Quaternion.LookRotation(splineDragging.GetPoint(offsetTime) - spikeTarget.position), slerpConstantPath * Time.deltaTime * slerpTimeRotation);
                    spike.transform.rotation = newRotation;
                    //}

                    slerpTimeRotation = slerpTimeRotation + Time.deltaTime * slerpTimeChargeupPathRotation;
                } else {
                    tracingPath = false;
                    releasingPlayer = true;
                    slerpTime = 0;
                    slerpTimeRotation = 0;
                    progress = 0;

                    Player.CanControlPlayer = true;

                    Player.PlayerInstance.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    Player.PlayerInstance.transform.SetParent(GameObject.FindGameObjectWithTag("GameController").transform);
               
                    Player.PlayerInstance.GetComponent<Rigidbody>().velocity = splineDragging.GetVelocity(1);
                    Player.PlayerInstance.transform.position += splineDragging.GetVelocity(1) * Time.fixedDeltaTime;
                    spikeRb.velocity = -splineDragging.GetVelocity(1);
                    spikeRb.transform.position -= splineDragging.GetVelocity(1) * Time.fixedDeltaTime;

                    spike.AddPushTarget(Player.PlayerMagnetic);
                    spike.SteelPushing = true;
                    spike.SteelBurnRateTarget = .5f;
                    spike.PushTargets.MaxRange = 3;
                }
            }

            // Pushing the player off of the Spike.
            if (releasingPlayer) {
                if ((Player.PlayerIronSteel.CenterOfMass - spike.CenterOfMass).sqrMagnitude > .01) {
                    Quaternion newRotation = Quaternion.LookRotation(Player.PlayerIronSteel.CenterOfMass - spike.CenterOfMass);
                    //Quaternion newRotation = Quaternion.LookRotation(-spikeRb.velocity);
                    spike.transform.rotation = newRotation;
                }

                if ((spike.CenterOfMass - Player.PlayerIronSteel.CenterOfMass).magnitude > distanceThresholdReturn) {
                    releasingPlayer = false;
                    returningHome = true;

                    spikeRb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    spikeTarget.transform.position = spike.transform.position;
                    spike.transform.localPosition = Vector3.zero;
                    splineReturnHome.SetInitialPoint(transform.InverseTransformPoint(spike.transform.position), spikeRb.velocity.normalized);

                }
            }

            if (returningHome) {
                progress += Time.deltaTime / animationTime;
                if (progress < 1) {
                    splineReturnHome.FollowCurve(spikeTarget.transform, progress, false);

                    // If the spike was pulled away from its path, force the spike back towards the path
                    Vector3 distance = spikeTarget.position - spike.CenterOfMass;
                    float sqrDistance = distance.sqrMagnitude;
                    spikeRb.AddForce(forceConstantFar * distance.normalized * sqrDistance, ForceMode.Acceleration);

                    // Rotate the spike to follow the path, a few moments in the future
                    float offsetTime = progress + forwardTimeOffset / animationTime;
                    offsetTime = offsetTime > 1 ? 1 : offsetTime;

                    Vector3 velocity = splineReturnHome.GetVelocity(offsetTime);
                    Quaternion newRotation = Quaternion.Slerp(spike.transform.rotation, Quaternion.LookRotation(splineReturnHome.GetPoint(offsetTime) - spikeTarget.position), slerpConstantPath * Time.deltaTime * slerpTimeRotation);
                    spike.transform.rotation = newRotation;

                    slerpTimeRotation = slerpTimeRotation + Time.deltaTime * slerpTimeChargeupPathReturn;
                } else {
                    returningHome = false;
                    anim.enabled = true;

                    startTime = 0;
                    slerpTime = 0;
                    slerpTimeRotation = 0;
                    progress = 0;

                    spikeRb.isKinematic = true;
                    anim.ResetTrigger("PlayerEntersHome");
                    anim.SetTrigger("Fall");
                }
            }
        }
    }

    private void FixedUpdate() {
        if (chasingPlayer || spikingPlayer) { // Make spike only every travel directly towards player
            Vector3 vel = spikeRb.velocity;
            vel = Vector3.Project(vel, (Player.PlayerIronSteel.CenterOfMass - spike.CenterOfMass).normalized);
            spikeRb.velocity = vel;
        }
    }

    private void OnTriggerStay(Collider other) {
        if (other.CompareTag("PlayerBody") && !other.isTrigger && anim.enabled) {
            anim.SetTrigger("PlayerEntersHome");
        }
    }

    private void FacePlayer() {
        facingPlayer = true;
    }

    private void TracePlayer() {
        facingPlayer = false;
        tracingPlayer = true;
        anim.enabled = false;
        startTime = Time.time;
    }
}
