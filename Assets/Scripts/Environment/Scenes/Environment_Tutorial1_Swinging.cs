using UnityEngine;
using System.Collections;
using static TextCodes;

public class Environment_Tutorial1_Swinging : EnvironmentCinematic {

    protected override IEnumerator Trigger0() {
        while (HUD.ConversationHUDController.IsOpen) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeIn(HowToControlWheel + " to open the " + ControlWheel + " and choose " + AreaMode + ".");
        FlagsController.SetFlag("wheel_area");
        while (!HUD.ControlWheelController.IsOpen)
            yield return null;
        HUD.MessageOverlayCinematic.FadeOut();
    }
}
