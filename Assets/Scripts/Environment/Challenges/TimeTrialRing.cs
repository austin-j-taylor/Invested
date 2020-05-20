using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTrialRing : MonoBehaviour
{
    [HideInInspector]
    public Challenge_TimeTrial trial; // set by the Challenge_TimeTrial
    public bool Passed { get; private set; }

    private void OnTriggerEnter(Collider other) {
        if(Player.IsPlayerTrigger(other)) {
            Passed = true;
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
            GetComponent<AudioSource>().Play();
        }
    }

}
