using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricLines;

public class Simulation_duel : MonoBehaviour {

    //private float timeToReset;
    private NonPlayerPushPullController[] allomancers;
    private Magnetic[] spheres;

    // Use this for initialization
    void Start() {
        allomancers = GetComponentsInChildren<NonPlayerPushPullController>();
        spheres = GetComponentsInChildren<Magnetic>();
        
        for(int i = 0; i < allomancers.Length; i++) {
            allomancers[i].AddPushTarget(spheres[i / 2]);
            allomancers[i].SteelPushing = true;
            allomancers[i].SteelBurnRateTarget = 1;
        }
    }
    //private void Update() {
    //    timeToReset += Time.deltaTime;
    //    if (timeToReset > 2) {
    //        int index = spheres.Length - 1;
    //        spheres[index].transform.localPosition = new Vector3(0, 0, 3);
    //        spheres[index].transform.rotation = Quaternion.identity;
    //        spheres[index].GetComponent<Rigidbody>().velocity = Vector3.zero;
    //        allomancers[allomancers.Length - 2].AddPushTarget(spheres[index]);
    //        allomancers[allomancers.Length - 1].AddPushTarget(spheres[index]);
    //        timeToReset = 0;
    //    }
    //}
}
