using UnityEngine;
using System.Collections;

public class Environment_Tutorial1_Swinging : Environment {


    [SerializeField]
    private MessageTrigger trigger = null;

    private void Start() {
        trigger.routine = Trigger();
    }

    private IEnumerator Trigger() {
        while (HUD.ConversationHUDController.IsOpen) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeIn(Messages.tutorial_controlwheel);
        HUD.ControlWheelController.SetLockedState(ControlWheelController.LockedState.LockedToArea);
        while (!HUD.ControlWheelController.IsOpen)
            yield return null;
        HUD.MessageOverlayCinematic.FadeOut();
    }
}
