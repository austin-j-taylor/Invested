using UnityEngine;
using System.Collections;

public class Environment_Tutorial1_Swinging : EnvironmentCinematic {

    protected override IEnumerator Trigger0() {
        while (HUD.ConversationHUDController.IsOpen) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeIn(Messages.tutorial_controlwheel);
        FlagsController.SetFlag("wheel_area");
        while (!HUD.ControlWheelController.IsOpen)
            yield return null;
        HUD.MessageOverlayCinematic.FadeOut();
    }
}
