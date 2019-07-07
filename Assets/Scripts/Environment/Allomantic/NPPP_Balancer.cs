using UnityEngine;
using System.Collections;

public class NPPP_Balancer : MonoBehaviour {

    NonPlayerPushPullController puller;
    NonPlayerPushPullController pusher;
    Magnetic target;
    Transform cubeAnchor;

    // Use this for initialization
    void Start() {
        NonPlayerPushPullController[] children = GetComponentsInChildren<NonPlayerPushPullController>();
        puller = children[0];
        pusher = children[1];
        target = GetComponentInChildren<Magnetic>();
        cubeAnchor = puller.transform.parent;

        puller.AddPullTarget(target);
        pusher.AddPushTarget(target);
        puller.IronPulling = true;
        pusher.SteelPushing = true;

    }

    private void Update() {
        if (!PauseMenu.IsPaused) {
            cubeAnchor.position = target.transform.position;
            if (target.Velocity.sqrMagnitude > 0.01f)
                cubeAnchor.transform.rotation = Quaternion.Slerp(cubeAnchor.transform.rotation, Quaternion.LookRotation(target.Velocity), 12 * Time.deltaTime);
            puller.transform.LookAt(target.transform.position);
            pusher.transform.LookAt(target.transform.position);
            //puller.transform.RotateAround(cubeAnchor.position, Vector3.up, Time.deltaTime * 30);
            //pusher.transform.RotateAround(cubeAnchor.position, Vector3.up, -Time.deltaTime * 360);
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        // Every frame, apply a force such that the net force acting on the object is equal/opposite to gravity
        Vector3 Fi = -puller.CalculateAllomanticForce(target);
        Vector3 Fs = pusher.CalculateAllomanticForce(target);

        float deltaI = Fi.x - Fi.z;
        float deltaS = Fs.x - Fs.z;

        float Fny = -Physics.gravity.y * target.NetMass;

        if(deltaI != 0 && deltaS != 0) {
            puller.IronBurnPercentageTarget =  Mathf.Clamp01(-Fny / (deltaI / deltaS * Fs.y - Fi.y));
            pusher.SteelBurnPercentageTarget = Mathf.Clamp01(Fny / (Fs.y - Fi.y * deltaS / deltaI));
        } else if(deltaS != 0) {
            puller.IronBurnPercentageTarget =  Mathf.Clamp01(Fny / -Fi.y);
            pusher.SteelBurnPercentageTarget = 0;
        } else if(deltaI != 0) {
            puller.IronBurnPercentageTarget =  0;
            pusher.SteelBurnPercentageTarget = Mathf.Clamp01(Fny / Fs.y);
        } else {
            puller.IronBurnPercentageTarget = 0;
            pusher.SteelBurnPercentageTarget = 0;
        }
        //Vector3 netforce = puller.IronBurnPercentageTarget * Fi + pusher.SteelBurnPercentageTarget * Fs;
        //Debug.Log("...");
        //Debug.Log(netforce);
        //Debug.Log(puller.IronBurnPercentageTarget + " , " + pusher.SteelBurnPercentageTarget);
        //Debug.Log(puller.IronBurnPercentageTarget * Fi.magnitude + " , " + pusher.SteelBurnPercentageTarget * Fs.magnitude);
        //Debug.Log(puller.LastAllomanticForce.magnitude + " , " + pusher.LastAllomanticForce.magnitude);
        //Debug.DrawRay(target.transform.position, Fi * puller.IronBurnPercentageTarget);
        //Debug.DrawRay(target.transform.position, Fs * pusher.SteelBurnPercentageTarget);
        //Debug.Log(target.Velocity);

    }
}
