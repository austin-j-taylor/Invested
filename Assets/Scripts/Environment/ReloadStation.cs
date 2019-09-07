using UnityEngine;
using System.Collections;

public class ReloadStation : MonoBehaviour {

    private const float refillAmount = 50;

    [SerializeField]
    private double capacity = 150;
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
            if(anim) anim.SetBool("activated", true);
        }
    }

    private void OnTriggerStay(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger) {
            if (
                    (!fillIron || Player.PlayerIronSteel.IronReserve.IsFullFuzzy) &&
                    (!fillSteel || Player.PlayerIronSteel.SteelReserve.IsFullFuzzy) &&
                    (!fillPewter || Player.PlayerPewter.PewterReserve.IsFullFuzzy)
            ) {
                if (anim) anim.SetBool("activated", false);
            } else {
                if (anim) anim.SetBool("activated", true);
            }

            if (fillIron)
                Player.PlayerIronSteel.IronReserve.Fill(refillAmount * Time.deltaTime, capacity);
            if(fillSteel)
                Player.PlayerIronSteel.SteelReserve.Fill(refillAmount * Time.deltaTime, capacity);
            if (fillPewter)
                Player.PlayerPewter.PewterReserve.Fill(refillAmount * Time.deltaTime, capacity);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger) {
            if (anim) anim.SetBool("activated", false);
        }
    }
}
