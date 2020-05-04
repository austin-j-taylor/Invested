using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Environment_Tutorial1 : Environment {

    void Start() {

        Player.CanControl = false;
        Player.CanControlMovement = false;
        Player.PlayerInstance.CoinHand.Pouch.Clear();
        Player.PlayerIronSteel.SteelReserve.IsEnabled = false;
        Player.PlayerPewter.PewterReserve.IsEnabled = false;
        Player.CanControlZinc = false;
        HUD.ControlWheelController.SetLockedState(ControlWheelController.LockedState.LockedFully);
        HUD.HelpOverlayController.SetLockedState(HelpOverlayController.LockedState.Locked0);

        StartCoroutine(Procedure());
    }

    private IEnumerator Procedure() {

        yield return new WaitForSeconds(2);
        Player.CanControl = true;
        Player.CanControlMovement = true;
    }
}
