using UnityEngine;
using System.Collections;
using static TextCodes;

public class Environment_Tutorial2_Bubble : EnvironmentWithTriggers {

    private void Start() {
        triggers[1].routine = ClearHUD();
    }

    protected override IEnumerator Trigger0() {
        while (HUD.ConversationHUDController.IsOpen)
            yield return null;
        HUD.ControlWheelController.SetLockedState(ControlWheelController.LockedState.LockedToBubble);
        HUD.MessageOverlayCinematic.FadeIn("Open the " + ControlWheel + " and choose " + Red("Bubble") + " to Push on all nearby metals.");
        while (!HUD.ControlWheelController.IsOpen)
            yield return null;
        HUD.MessageOverlayCinematic.FadeOut();

        yield break;
    }
}
