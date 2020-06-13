using UnityEngine;
using System.Collections;

// When the trigger is entered, set the flag.
public class FlagTrigger : MonoBehaviour {

    [SerializeField]
    private string flag = "";

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger) {
            if(flag != "")
                FlagsController.SetFlag(flag);
            enabled = false;
        }
    }
}
