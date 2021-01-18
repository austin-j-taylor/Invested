using UnityEngine;
using System.Collections;
using static TextCodes;

public class Environment_Tutorial2_Bubble : EnvironmentWithTriggers {

    protected override IEnumerator Trigger0() {
        while (HUD.ConversationHUDController.IsOpen)
            yield return null;
        FlagsController.SetFlag("wheel_bubble");
        HUD.MessageOverlayCinematic.FadeIn("Open the " + ControlWheel + " and choose " + BubbleMode + " to Push on all nearby metals.");
        while (!Prima.PrimaInstance.ActorIronSteel.BubbleIsOpen)
            yield return null;
        HUD.MessageOverlayCinematic.FadeOut();
    }
}
