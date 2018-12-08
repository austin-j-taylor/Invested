using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikePit : BezierCurve {
    

    private const float distanceThreshold = .01f;
    private const float forwardTimeOffset = .2f;
    private float progress = 0;

    private const float forceConstantFar = 40f;

    private Spike pitSpike;
    private Rigidbody pitSpikeRb;
    private Transform spikeTarget;

    void Awake() {
        pitSpike = GetComponentInChildren<Spike>();
        pitSpikeRb = pitSpike.GetComponent<Rigidbody>();
        spikeTarget = pitSpike.transform.parent;

        pitSpike.transform.localPosition = pitSpikeRb.centerOfMass;
    }

    void Update() {
        if (!PauseMenu.IsPaused) {
            progress += Time.deltaTime / AnimationTime;
            if (progress > 1f) {
                progress = progress % 1;
            }

            FollowCurve(spikeTarget.transform, progress, false);

            // If the spike was pulled away from its path, force the spike back towards the path
            Vector3 distance = spikeTarget.position - pitSpike.CenterOfMass;
            float sqrDistance = distance.sqrMagnitude;
            if (sqrDistance > distanceThreshold)
                pitSpikeRb.AddForce(forceConstantFar * distance.normalized * sqrDistance, ForceMode.Acceleration);
            //Debug.Log(distance);
            // Rotate the spike to follow the path, a few moments in the future
            Vector3 velocity = GetVelocity(progress + forwardTimeOffset / AnimationTime);
            if (velocity.sqrMagnitude > distanceThreshold)
                pitSpike.transform.rotation = Quaternion.LookRotation(GetPoint(progress + forwardTimeOffset / AnimationTime) - pitSpike.CenterOfMass);
                //pitSpike.transform.LookAt(GetPoint(progress + forwardTimeOffset / AnimationTime));
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            Debug.Log("PLAYER INSIDE");

        }
    }
}
