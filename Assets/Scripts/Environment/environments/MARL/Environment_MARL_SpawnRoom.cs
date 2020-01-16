using UnityEngine;
using System.Collections;

public class Environment_MARL_SpawnRoom : Environment {
    
    // Environment members
    [SerializeField]
    private Animator flickeringSpotlight = null;
    [SerializeField]
    private FacilityDoor door = null;
    

    private void Start() {
        Player.CanControlZinc = false;
        HUD.ControlWheelController.SetLockedState(ControlWheelController.LockedState.LockedFully);
        HUD.HelpOverlayController.SetLockedState(HelpOverlayController.LockedState.Locked0);
        StartCoroutine(Procedure());
    }

    // State coroutines
    private IEnumerator Procedure() {
        Player.CanControl = false;
        Player.CanControlMovement = false;
        Player.PlayerInstance.CoinHand.Pouch.Clear();
        Player.PlayerIronSteel.IronReserve.SetMass(100);
        Player.PlayerIronSteel.SteelReserve.SetMass(0);
        Player.PlayerPewter.PewterReserve.SetMass(0);

        yield return new WaitForSeconds(3);

        flickeringSpotlight.SetTrigger("flickerOn");
        yield return new WaitForSeconds(2);

        if (CameraController.HasNotMovedCamera) {
            HUD.MessageOverlayCinematic.FadeIn(Messages.tutorial_movement); // look
            while (CameraController.HasNotMovedCamera)
                yield return null;


            HUD.MessageOverlayCinematic.FadeOut();
            yield return new WaitForSeconds(5);
        } else {
            HUD.MessageOverlayCinematic.FadeIn(Messages.tutorial_movement); // look
            HUD.MessageOverlayCinematic.FadeOut();
        }
        HUD.MessageOverlayCinematic.Next(); // start burning

        Player.CanControl = true;

        while (!Player.PlayerIronSteel.IsBurning)
            yield return null;


        HUD.MessageOverlayCinematic.FadeOut();
        yield return new WaitForSeconds(3);
        HUD.MessageOverlayCinematic.Next(); // pull

        while (!Player.PlayerIronSteel.IronPulling)
            yield return null;

        HUD.MessageOverlayCinematic.FadeOut();

        while (door.On)
            yield return null;

        Player.CanControlMovement = true;

        yield return new WaitForSeconds(2);
        if(Player.PlayerInstance.GetComponent<Rigidbody>().velocity.sqrMagnitude < .25f) {
            HUD.MessageOverlayCinematic.Next(); // move, and space to jump

            while (Player.PlayerInstance.GetComponent<Rigidbody>().velocity.sqrMagnitude < .25f)
                yield return null;

            HUD.MessageOverlayCinematic.Next();
        } else {
            HUD.MessageOverlayCinematic.Next();
            HUD.MessageOverlayCinematic.FadeOut();
        }
        

        // player leaves room
        // door closes when player crosses trigger
        enabled = false;
    }
    // other coroutines when triggers are hit or other actions are done
    // these coroutines may be public and started elsewhere

}
