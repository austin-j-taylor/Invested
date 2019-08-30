using UnityEngine;
using System.Collections;

public class Environment_MARL_SpawnRoom : Environment {

    // Environment members
    [SerializeField]
    private Light flickeringSpotlight = null;
    [SerializeField]
    private FacilityDoor door = null;

    private void Start() {
        StartCoroutine(startingControls());
    }
    
    void Update() {
        flickeringSpotlight.intensity = 17 * Mathf.Sin(Time.time);
    }

    // State coroutines
    private IEnumerator startingControls() {
        Player.CanControlMovement = false;
        Player.CanControlPushes = false;

        yield return new WaitForSeconds(3);
        if(CameraController.HasNotMovedCamera) {
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

        Player.CanControlPushes = true;

        while (!Player.PlayerIronSteel.IsBurning)
            yield return null;


        HUD.MessageOverlayCinematic.FadeOut();
        yield return new WaitForSeconds(3);
        HUD.MessageOverlayCinematic.Next(); // pull

        while (!Player.PlayerIronSteel.IronPulling)
            yield return null;

        HUD.MessageOverlayCinematic.FadeOut();
        Player.CanControlMovement = true;

        while (door.On)
            yield return null;

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

    }
    // other coroutines when triggers are hit or other actions are done
    // these coroutines may be public and started elsewhere

}
