using UnityEngine;
using System.Collections;

public class Environment_Tutorial1_Marking : EnvironmentCinematic {

    [SerializeField]
    FacilityDoor door0 = null;
    [SerializeField]
    FacilityDoor door1 = null;
    [SerializeField]
    FacilityDoor door2 = null;


    protected override IEnumerator Trigger0() {
        HUD.MessageOverlayCinematic.FadeIn(Messages.tutorial_mark);
        while (door0.On) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOut();
    }
    protected override IEnumerator Trigger1() {
        HUD.MessageOverlayCinematic.Next();
        while (door1.On) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOut();
    }
    protected override IEnumerator Trigger2() {
        HUD.MessageOverlayCinematic.Next();
        while (door2.On) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.Next();
    }
}
