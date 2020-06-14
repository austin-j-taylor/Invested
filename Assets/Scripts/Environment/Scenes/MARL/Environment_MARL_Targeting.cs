using UnityEngine;
using System.Collections;

public class Environment_MARL_Targeting : Environment {

    [SerializeField]
    FacilityDoor door0 = null;
    [SerializeField]
    FacilityDoor door1 = null;
    [SerializeField]
    FacilityDoor door2 = null;

    private BoxCollider[] cols;

    private void Start() {
        cols = GetComponentsInChildren<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger) {
            // delete references to door0/1/2 as we pass them to know where we are
            // yes this is dumb
            if (door0) {
                StartCoroutine(Procedure0());
                cols[0].enabled = false;
            } else if (door1) {
                StartCoroutine(Procedure1());
                cols[1].enabled = false;
            } else if (door2) {
                StartCoroutine(Procedure2());
                cols[2].enabled = false;
            }
        }
    }

    // State coroutines
    private IEnumerator Procedure0() {
        //HUD.MessageOverlayCinematic.FadeIn(Messages.marl_targeting);
        while (door0.On) {
            yield return null;
        }
        door0 = null;
    }
    private IEnumerator Procedure1() {
        HUD.MessageOverlayCinematic.Next();
        while (door1.On) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOut();
        door1 = null;
    }
    private IEnumerator Procedure2() {
        HUD.MessageOverlayCinematic.Next();
        while (door2.On) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.Next();
        enabled = false;
        door2 = null;
    }
}
