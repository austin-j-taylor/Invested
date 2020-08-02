using UnityEngine;
using System.Collections;
using static TextCodes;

public class Environment_Tutorial1_Marking : EnvironmentCinematic {

    [SerializeField]
    FacilityDoor door0 = null;
    [SerializeField]
    FacilityDoor door1 = null;
    //[SerializeField]
    //FacilityDoor door2 = null;


    protected override IEnumerator Trigger0() {
        HUD.MessageOverlayCinematic.FadeIn(HowToMark_Pull + " while looking at a metal to " + Mark_pulling + " it for " + Pulling + ".\nYou can " + Pull + " on a " + MarkedMetal + " without looking at it.");
        while (door0.On) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOut();
    }
    protected override IEnumerator Trigger1() {
        HUD.MessageOverlayCinematic.FadeOutInto(HowToMultiMark_Pull + " to " + Mark_pulling + " multiple metals at once.");
        while (door1.On) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOut();
    }
    protected override IEnumerator Trigger2() {
        //HUD.MessageOverlayCinematic.FadeOutInto("You can " + Pull + " on many metals simultaneously by " + Marking_pulling + " them.");
        //while (door2.On) {
        //    yield return null;
        //}
        //HUD.MessageOverlayCinematic.FadeOut();
        yield break;
    }
}
