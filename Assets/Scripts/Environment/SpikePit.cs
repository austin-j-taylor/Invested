using UnityEngine;
using System.Collections;

public class SpikePit : MonoBehaviour {

    private const float slerpConstantPlayer = 8f;
    private const float slerpConstantPath = 4f;
    private const float slerpTimeChargeupPlayer = 2f;
    private const float slerpTimeChargeupPath = .5f;
    private const float equalAngleThresholdPlayer = .075f;
    private const float equalAngleThresholdPath = 5f;
    private const float equalDistanceThreshold = .5f;
    private const float distanceThreshold = .00001f;
    private const float forwardTimeOffset = .2f;
    private const float forceConstantFar = 40f;
    private const float animationTime = 4;

    private bool tracingPlayer = false;
    private bool chasingPlayer = false;
    private bool tracingPath = false;
    private bool followingPath = false;
    private float slerpTime;
    private float progress;
    private Quaternion initialRotation;
    private Quaternion initialVelocity;

    private Animator anim;
    private Transform spikeTarget;
    private Rigidbody spikeRb;
    private NonPlayerPushPullController spike;
    private Transform playerAnchor;
    private SpikeSpline splineDragging;
    private SpikeSpline splineReturnHome;

    void Start() {
        anim = GetComponent<Animator>();
        spike = GetComponentInChildren<NonPlayerPushPullController>();
        spikeRb = spike.GetComponent<Rigidbody>();
        spikeTarget = spike.transform.parent;
        playerAnchor = spike.transform.GetChild(0);
        SpikeSpline[] splines = GetComponentsInChildren<SpikeSpline>();
        splineDragging = splines[0];
        splineReturnHome = splines[1];

        spike.LinesAreVisibleWhenNotBurning = true;


        tracingPlayer = false;
        chasingPlayer = false;
        tracingPath = false;
        followingPath = false;
        slerpTime = 0;
        progress = 0;
    }

    private void Update() {
        if (!PauseMenu.IsPaused) {

            if (tracingPlayer) {
                Quaternion newRotation = Quaternion.Slerp(spikeTarget.rotation, Quaternion.LookRotation(Player.PlayerInstance.transform.position - spikeTarget.position), slerpConstantPlayer * Time.deltaTime * slerpTime);
                slerpTime = slerpTime + Time.deltaTime * slerpTimeChargeupPlayer;
                spikeTarget.rotation = newRotation;
                spike.transform.position = spikeTarget.transform.position;

                float angle = Quaternion.Angle(Quaternion.LookRotation(Player.PlayerInstance.transform.position - spikeTarget.position), newRotation);
                if (angle < equalAngleThresholdPlayer) {
                    slerpTime = 0;
                    tracingPlayer = false;
                    chasingPlayer = true;
                    spikeRb.velocity = Vector3.zero;
                    spike.IronBurnRateTarget = .75f;

                }
            }

            if (chasingPlayer) {

                Quaternion newRotation = Quaternion.LookRotation(Player.PlayerIronSteel.CenterOfMass - spike.transform.position);
                spike.transform.rotation = newRotation;

                if ((Player.PlayerIronSteel.CenterOfMass - spike.CenterOfMass).magnitude < equalDistanceThreshold) {
                    chasingPlayer = false;
                    tracingPath = true;

                    // Set Player Anchor posiiton
                    Player.PlayerInstance.GetComponent<Rigidbody>().isKinematic = true;

                    // Make Spike Target rotate towards spline velocity
                    spikeTarget.transform.position = spike.transform.position;
                    spike.transform.localPosition = Vector3.zero;
                    spikeTarget.transform.rotation = spike.transform.rotation;
                    spike.transform.localRotation = Quaternion.identity;
                    spike.IronBurnRateTarget = 0;
                    spike.IronPulling = false;
                    initialRotation = spikeTarget.rotation;
                    playerAnchor.position = Player.PlayerInstance.transform.position;
                    initialVelocity = Quaternion.LookRotation(splineDragging.GetPoint(forwardTimeOffset / animationTime) - spike.CenterOfMass);
                    splineDragging.SetInitialPoint(transform.InverseTransformPoint(spike.transform.position));
                }
            }

            if (tracingPath) {
                progress += Time.deltaTime / animationTime * slerpTime;
                slerpTime = Mathf.Min(1, slerpTime + Time.deltaTime * slerpTimeChargeupPath);
                if (progress < 1f) {
                    splineDragging.FollowCurve(spikeTarget.transform, progress, false);

                    // If the spike was pulled away from its path, force the spike back towards the path
                    Vector3 distance = spikeTarget.position - spike.CenterOfMass;
                    float sqrDistance = distance.sqrMagnitude;
                    if (sqrDistance > distanceThreshold)
                        spikeRb.AddForce(forceConstantFar * distance.normalized * sqrDistance, ForceMode.Acceleration);

                    // Rotate the spike to follow the path, a few moments in the future
                    float offsetTime = progress + forwardTimeOffset / animationTime;

                    if (offsetTime < 1) {
                        Vector3 velocity = splineDragging.GetVelocity(offsetTime);
                        if (velocity.sqrMagnitude > distanceThreshold) {
                            Quaternion newRotation = Quaternion.Slerp(spikeTarget.rotation, Quaternion.LookRotation(splineDragging.GetPoint(offsetTime) - spike.CenterOfMass), slerpConstantPath * Time.deltaTime * slerpTime);
                            spikeTarget.rotation = newRotation;
                        }
                    }

                    offsetTime = offsetTime > 1 ? 1 : offsetTime;
                } else {
                    followingPath = false;
                }
            }


            if (tracingPath || followingPath) {
                // Update player's position
                Player.PlayerInstance.transform.position = playerAnchor.transform.position;
                Player.PlayerInstance.transform.rotation = playerAnchor.transform.rotation;
            }
        }
    }

    private void FixedUpdate() {
        if (chasingPlayer) { // Make spike only every travel directly towards player
            Vector3 vel = spikeRb.velocity;
            vel = Vector3.Project(vel, (Player.PlayerIronSteel.CenterOfMass - spike.CenterOfMass).normalized);
            spikeRb.velocity = vel;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("PlayerBody")) {
            anim.SetTrigger("PlayerEntersHome");
        }
    }

    private void TracePlayer() {
        anim.enabled = false;
        // add player as a pull target
        spike.AddPullTarget(Player.PlayerMagnetic);
        spike.IronPulling = true;
        spike.IronBurnRateTarget = .2f;
        tracingPlayer = true;
        initialRotation = spikeTarget.rotation;
    }
}
