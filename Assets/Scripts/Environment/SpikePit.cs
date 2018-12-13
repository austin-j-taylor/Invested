using UnityEngine;
using System.Collections;

public class SpikePit : MonoBehaviour {

    private const float slerpConstant = 8f;
    private const float equalAngleThreshold = .05f;
    private const float equalDistanceThreshold = .05f;
    private const float distanceThreshold = .00001f;
    private const float forwardTimeOffset = .2f;
    private const float forceConstantFar = 40f;

    private bool spikingPlayer = false;
    private bool followingPath = false;
    private float progress = 0;
    private float animationTime = 4;

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
    }

    private void LateUpdate() {
        if (!PauseMenu.IsPaused) {
            if (followingPath) {
                progress += Time.deltaTime / animationTime;
                if (progress < 1f) {
                    splineDragging.FollowCurve(spikeTarget.transform, progress, true);

                    // If the spike was pulled away from its path, force the spike back towards the path
                    Vector3 distance = spikeTarget.position - spike.CenterOfMass;
                    float sqrDistance = distance.sqrMagnitude;
                    if (sqrDistance > distanceThreshold)
                        spikeRb.AddForce(forceConstantFar * distance.normalized * sqrDistance, ForceMode.Acceleration);

                    // Rotate the spike to follow the path, a few moments in the future
                    float offsetTime = progress + forwardTimeOffset / animationTime;

                    if(offsetTime < 1) {
                        Vector3 velocity = splineDragging.GetVelocity(offsetTime);
                        if (velocity.sqrMagnitude > distanceThreshold)
                            spike.transform.rotation = Quaternion.LookRotation(splineDragging.GetPoint(offsetTime) - spike.CenterOfMass);
                    }

                    offsetTime = offsetTime > 1 ? 1 : offsetTime;

                } else {
                    followingPath = false;
                }
            }
        }
    }

    private void FixedUpdate() {
        if (spikingPlayer) {
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
        StartCoroutine(TracePlayerCoroutine());
    }

    IEnumerator TracePlayerCoroutine() {
        float angle = 90;
        do {
            yield return null;

            Quaternion oldRotation = spikeTarget.rotation;
            Quaternion newRotation = Quaternion.Slerp(oldRotation, Quaternion.LookRotation(Player.PlayerInstance.transform.position - spikeTarget.position), slerpConstant * Time.deltaTime);
            spikeTarget.rotation = newRotation;

            angle = Quaternion.Angle(oldRotation, newRotation);
            spike.transform.position = spikeTarget.transform.position;
        } while (angle > equalAngleThreshold);

        SpikePlayer();
    }

    private void SpikePlayer() {
        spikingPlayer = true;
        spikeRb.velocity = Vector3.zero;
        spike.IronBurnRateTarget = .75f;
        StartCoroutine(SpikePlayerCoroutine());
    }

    IEnumerator SpikePlayerCoroutine() {
        do {
            Quaternion newRotation = Quaternion.LookRotation(Player.PlayerIronSteel.CenterOfMass - spike.transform.position);
            spike.transform.rotation = newRotation;

            yield return null;
        } while ((Player.PlayerIronSteel.CenterOfMass - spike.CenterOfMass).sqrMagnitude > equalDistanceThreshold);

        splineDragging.SetInitialPoint(transform.InverseTransformPoint(spike.transform.position));
        Player.PlayerInstance.GetComponent<Rigidbody>().isKinematic = true;
        Player.PlayerInstance.transform.parent = spike.transform;
        spikeTarget.transform.position = spike.transform.position;
        spikeRb.velocity = Vector3.zero;
        spike.transform.localPosition = Vector3.zero;
        spike.transform.localRotation = Quaternion.identity;
        spike.IronBurnRateTarget = 0;
        spike.IronPulling = false;
        spikingPlayer = false;
        followingPath = true;
    }
}
