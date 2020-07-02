using UnityEngine;
using System.Collections;

/// <summary>
/// When the trigger is entered, set the flag.
/// </summary>
public class FlagTrigger : MonoBehaviour {

    [SerializeField]
    private string flag = "";

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger && GetComponentInParent<HarmonyTarget>().Unlocked) {
            if (flag != "")
                FlagsController.SetFlag(flag);
            enabled = false;
        }
    }
}
