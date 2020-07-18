using UnityEngine;
using System.Collections;
using VolumetricLines;

public class SpikeSplineFollower : SpikeSpline {

    private const float lineWidth = .1f;
    private const int stepsPerCurve = 30;
    private const float distanceThreshold = .01f;
    private const float forwardTimeOffset = .2f;
    private const float luminosityFactor = .5f;

    private const float forceConstantFar = 40f;

    protected Spike spike;
    protected Rigidbody spikeRb;
    protected Transform spikeTarget;
    private VolumetricLineStripBehavior volLines;
    private float progress = 0;

    void Start() {
        spike = GetComponentInChildren<Spike>();
        spikeRb = spike.GetComponent<Rigidbody>();
        spikeTarget = spike.transform.parent;
        spike.transform.localPosition = spikeRb.centerOfMass;

        int steps = stepsPerCurve * CurveCount;
        Vector3[] points = new Vector3[steps + 1];

        for (int i = 0; i <= steps; i++) {
            points[i] = GetPoint(i / (float)steps);
        }

        volLines = Instantiate(GameManager.MetalLineStripTemplate, GameManager.MetalLinesTransform);
        volLines.GetComponent<MeshRenderer>().enabled = true;
        volLines.UpdateLineVertices(points);
        volLines.LineColor = new Color(0, Magnetic.lowLineColor * luminosityFactor, Magnetic.highLineColor * luminosityFactor, 1);
        volLines.LightSaberFactor = 1;
        volLines.LineWidth = lineWidth;
    }

    void Update() {
        if (!GameManager.MenusController.pauseMenu.IsOpen) {
            progress += Time.deltaTime / AnimationTime;
            if (progress > 1f) {
                progress = progress % 1;
            }

            FollowCurve(spikeTarget.transform, progress, false);

            // If the spike was pulled away from its path, force the spike back towards the path
            Vector3 distance = spikeTarget.position - spike.CenterOfMass;
            float sqrDistance = distance.sqrMagnitude;
            if (sqrDistance > distanceThreshold)
                spikeRb.AddForce(forceConstantFar * distance.normalized * sqrDistance, ForceMode.Acceleration);
            //Debug.Log(distance);
            // Rotate the spike to follow the path, a few moments in the future
            Vector3 velocity = GetVelocity(progress + forwardTimeOffset / AnimationTime);
            if (velocity.sqrMagnitude > distanceThreshold)
                spike.transform.rotation = Quaternion.LookRotation(GetPoint(progress + forwardTimeOffset / AnimationTime) - spike.CenterOfMass);
            //pitSpike.transform.LookAt(GetPoint(progress + forwardTimeOffset / AnimationTime));
        }
    }
}