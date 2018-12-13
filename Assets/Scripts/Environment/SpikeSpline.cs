using UnityEngine;

public class SpikeSpline : BezierCurve {

    protected Spike spike;
    protected Rigidbody spikeRb;
    protected Transform spikeTarget;

    void Awake() {
        spike = GetComponentInChildren<Spike>();
        spikeRb = spike.GetComponent<Rigidbody>();
        spikeTarget = spike.transform.parent;
        spike.transform.localPosition = spikeRb.centerOfMass;
    }
}
