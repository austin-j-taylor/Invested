using UnityEngine;
using System.Collections;

public class Environment_MARL : MonoBehaviour {

    // Environment members
    [SerializeField]
    private FacilityDoor door = null;

    private void Start() {
        StartCoroutine(startingControls());
    }

    // State coroutines
    private IEnumerator startingControls() {
        Player.CanControlPlayer = false;
        Player.CanControlPushes = false;

        yield return new WaitForSeconds(3);
        HUD.MessageOverlayCinematic.FadeIn(Messages.movement[0]); // look
        while (CameraController.HasNotMovedCamera)
			yield return null;


        HUD.MessageOverlayCinematic.FadeOut();
        yield return new WaitForSeconds(3);
        HUD.MessageOverlayCinematic.FadeIn(Messages.movement[1]); // start

        Player.CanControlPushes = true;

        while (!Player.PlayerIronSteel.IsBurning)
			yield return null;


        HUD.MessageOverlayCinematic.FadeOut();
        HUD.MessageOverlayCinematic.FadeIn(Messages.movement[2]); // pull

        while (door.IsOn)
                yield return null;
        
        HUD.MessageOverlayCinematic.FadeOut();
        HUD.MessageOverlayCinematic.FadeIn(Messages.movement[3]); // move, and space to jump

        Player.CanControlPlayer = true;


        // player leaves room
        // door closes when player crosses trigger

    }
    // other coroutines when triggers are hit or other actions are done
    // these coroutines may be public and started elsewhere

}
