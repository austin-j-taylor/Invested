using UnityEngine;
using System.Collections;

public class SpikePit : MonoBehaviour {

    private const float slerpConstant = 8f;
    private const float equalThreshold = .05f;

    private bool tracingPlayer = false;

    private Animator anim;
    private Transform spikeTarget;
    private NonPlayerPushPullController spike;
    private SpikeSpline splineDragging;
    private SpikeSpline splineReturnHome;

    void Start() {
        anim = GetComponent<Animator>();
        spike = GetComponentInChildren<NonPlayerPushPullController>();
        spikeTarget = spike.transform.parent;
        SpikeSpline[] splines = GetComponentsInChildren<SpikeSpline>();
        splineDragging = splines[0];
        splineReturnHome = splines[1];

        spike.LinesAreVisibleWhenNotBurning = true;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("PlayerBody")) {
            anim.SetTrigger("PlayerEntersHome");
        }
    }


    private void LateUpdate() {
        if (tracingPlayer) {
            //Quaternion oldRotation = spikeTarget.rotation;
            //Quaternion newRotation = Quaternion.Slerp(oldRotation, Quaternion.LookRotation(Player.PlayerInstance.transform.position - spikeTarget.position), slerpConstant * Time.deltaTime);
            //spikeTarget.rotation = newRotation;

            //float angle = Quaternion.Angle(oldRotation, newRotation);
            //if (angle < equalThreshold) {
            //    // Look directly at player and stab them
            //    tracingPlayer = false;
            //    newRotation = Quaternion.LookRotation(Player.PlayerIronSteel.CenterOfMass - spikeTarget.position);
            //    spikeTarget.rotation = newRotation;
            //    Player.PlayerInstance.GetComponent<Rigidbody>().isKinematic = true;
            //}

        }
        spike.transform.position = spikeTarget.transform.position;
    }

    private void SpikePlayer() {
        tracingPlayer = true;
        StartCoroutine(TracePlayer());
        // add player as a pull target
        spike.AddPullTarget(Player.PlayerMagnetic);
        spike.IronPulling = true;
        spike.IronBurnRateTarget = .2f;
    }

    IEnumerator TracePlayer() {
        while (true) {

            Quaternion oldRotation = spikeTarget.rotation;
            Quaternion newRotation = Quaternion.Slerp(oldRotation, Quaternion.LookRotation(Player.PlayerInstance.transform.position - spikeTarget.position), slerpConstant * Time.deltaTime);
            spikeTarget.rotation = newRotation;

            float angle = Quaternion.Angle(oldRotation, newRotation);
            if (angle < equalThreshold) {
                // Look directly at player and stab them
                tracingPlayer = false;
                newRotation = Quaternion.LookRotation(Player.PlayerIronSteel.CenterOfMass - spikeTarget.position);
                spikeTarget.rotation = newRotation;
                //Player.PlayerInstance.GetComponent<Rigidbody>().isKinematic = true;
                break;
            } else {
                yield return null;
            }
        }
    }


}
