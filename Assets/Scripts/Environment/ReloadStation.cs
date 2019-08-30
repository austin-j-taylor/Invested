using UnityEngine;
using System.Collections;

public class ReloadStation : MonoBehaviour {

    private const float refillAmount = 50;

    [SerializeField]
    private bool fillIron = true;
    [SerializeField]
    private bool fillSteel = false;
    [SerializeField]
    private bool fillPewter = false;


    private Animator anim;

    private void Awake() {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger) {
            anim.SetBool("activated", true);
        }
    }

    private void OnTriggerStay(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger) {
            if (
                    (!fillIron || Player.PlayerIronSteel.IronReserve.IsFullFuzzy) &&
                    (!fillSteel || Player.PlayerIronSteel.SteelReserve.IsFullFuzzy) &&
                    (!fillPewter || Player.PlayerPewter.PewterReserve.IsFullFuzzy)
            ) {
                anim.SetBool("activated", false);
            } else {
                anim.SetBool("activated", true);
            }

            if (fillIron)
                Player.PlayerIronSteel.IronReserve.Mass += refillAmount * Time.deltaTime;
            if(fillSteel)
                Player.PlayerIronSteel.SteelReserve.Mass += refillAmount * Time.deltaTime;
            if (fillPewter)
                Player.PlayerPewter.PewterReserve.Mass += refillAmount * Time.deltaTime;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger) {
            anim.SetBool("activated", false);
        }
    }
}
