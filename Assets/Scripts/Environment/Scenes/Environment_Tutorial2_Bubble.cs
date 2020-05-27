using UnityEngine;
using System.Collections;
using static TextCodes;

public class Environment_Tutorial2_Bubble : EnvironmentWithTriggers {

    private void Start() {
        triggers[2].routine = ClearHUD();
    }

    protected override IEnumerator Trigger0() {
        while (HUD.ConversationHUDController.IsOpen)
            yield return null;
        HUD.ControlWheelController.SetLockedState(ControlWheelController.LockedState.LockedToBubble);
        HUD.MessageOverlayCinematic.FadeIn("Open the " + ControlWheel + " and choose " + Red("Bubble") + " to Push on all nearby metals.");
        while (!HUD.ControlWheelController.IsOpen)
            yield return null;
        HUD.MessageOverlayCinematic.FadeOut();
    }

    protected override IEnumerator Trigger1() {
        HUD.MessageOverlayCinematic.FadeIn(KeyNumberOfTargets + " to increase the radius of the bubble.");
        yield return new WaitForSeconds(5);
        HUD.MessageOverlayCinematic.FadeOut();
        yield break;
    }
}
