using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikePit : BezierCurve {

    private const float animationTime = 5;
    private float progress = 0;

    private Spike pitSpike;
    //private Transform spikePuller;
    
    void Start() {
        pitSpike = GetComponentInChildren<Spike>();
        //spikePuller = GetComponentInChildren<AllomanticIronSteel>();
    }

    void Update() {
        progress += Time.deltaTime / animationTime;
        if (progress > 1f) {
            progress = progress % 1;
        }

        FollowCurve(pitSpike.transform, progress);
        //CameraController.ActiveCamera.transform.LookAt(pitSpike.transform);
    }
}
