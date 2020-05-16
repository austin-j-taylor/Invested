using UnityEngine;
using System.Collections;

public class Environment_Tutorial1_Marking : Environment {

    [SerializeField]
    FacilityDoor door0 = null;
    [SerializeField]
    FacilityDoor door1 = null;
    [SerializeField]
    FacilityDoor door2 = null;

    [SerializeField]
    private MessageTrigger trigger_mark1 = null,
                            trigger_mark2 = null,
                            trigger_mark3 = null;

    private void Start() {
        trigger_mark1.routine = Trigger_Mark1();
        trigger_mark2.routine = Trigger_Mark2();
        trigger_mark3.routine = Trigger_Mark3();
    }

    private IEnumerator Trigger_Mark1() {
        HUD.MessageOverlayCinematic.FadeIn(Messages.tutorial_mark);
        while (door0.On) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOut();
    }
    private IEnumerator Trigger_Mark2() {
        HUD.MessageOverlayCinematic.Next();
        while (door1.On) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOut();
    }
    private IEnumerator Trigger_Mark3() {
        HUD.MessageOverlayCinematic.Next();
        while (door2.On) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.Next();
    }
}
