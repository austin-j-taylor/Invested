using UnityEngine;
using System.Collections;

public class Environment_MARL_Pushes : Environment {

    private void Start() {
        StartCoroutine(Procedure());
    }

    // State coroutines
    private IEnumerator Procedure() {
        Player.CanControlZinc = false;
        Player.CanControlWheel = false;
        Player.PlayerInstance.CoinHand.Pouch.Clear();
        Player.PlayerIronSteel.IronReserve.SetMass(100);
        Player.PlayerIronSteel.SteelReserve.SetMass(0);
        Player.PlayerPewter.PewterReserve.SetMass(0);

        yield return null;
        // player leaves room
        // door closes when player crosses trigger
        enabled = false;
    }

    // other coroutines when triggers are hit or other actions are done
    // these coroutines may be public and started elsewhere

}
