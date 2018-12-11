using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricLines;

public class SpikeSpline : BezierCurve {
    

    private const float distanceThreshold = .01f;
    private const float forwardTimeOffset = .2f;
    private float progress = 0;

    private const float forceConstantFar = 40f;

    private Spike pitSpike;
    private Rigidbody pitSpikeRb;
    private Transform spikeTarget;
    private VolumetricLineStripBehavior volLines;

    void Start() {
        pitSpike = GetComponentInChildren<Spike>();
        pitSpikeRb = pitSpike.GetComponent<Rigidbody>();
        spikeTarget = pitSpike.transform.parent;
        pitSpike.transform.localPosition = pitSpikeRb.centerOfMass;

        int steps = BezierCurveEditor.stepsPerCurve * CurveCount;
        Vector3[] points = new Vector3[steps + 1];

        for(int i = 0; i <= steps; i++) {
            points[i] = GetPoint(i / (float)steps);
        }

        volLines = Instantiate(GameManager.MetalLineStripTemplate);
        volLines.GetComponent<MeshRenderer>().enabled = true;
        volLines.UpdateLineVertices(points);
        volLines.LineColor = new Color(0, AllomanticIronSteel.lowLineColor, AllomanticIronSteel.highLineColor, 1);
        volLines.LightSaberFactor = 1;
        volLines.LineWidth = AllomanticIronSteel.blueLineWidthBaseFactor;
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
            //Debug.Log("PLAYER INSIDE");

        }
    }
}
