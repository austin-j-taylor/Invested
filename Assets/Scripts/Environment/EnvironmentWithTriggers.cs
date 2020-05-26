using UnityEngine;
using UnityEditor;
using System.Collections;

// An environment with (up to 6) triggers.
public class EnvironmentWithTriggers : Environment {

    [SerializeField]
    protected MessageTrigger[] triggers = null;

    protected virtual void Awake() {
        if(triggers != null) {
            int len = triggers.Length;
            if (len > 0) {
                triggers[0].routine = Trigger0();
                if (len > 1) {
                    triggers[1].routine = Trigger1();
                    if (len > 2) {
                        triggers[2].routine = Trigger2();
                        if (len > 3) {
                            triggers[3].routine = Trigger3();
                            if (len > 4) {
                                triggers[4].routine = Trigger4();
                                if (len > 5) {
                                    triggers[5].routine = Trigger5();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // For triggers that simply clear text from the HUD
    protected IEnumerator ClearHUD() {
        HUD.MessageOverlayCinematic.FadeOut();
        yield break;
    }

    protected virtual IEnumerator Trigger0() { yield break; }
    protected virtual IEnumerator Trigger1() { yield break; }
    protected virtual IEnumerator Trigger2() { yield break; }
    protected virtual IEnumerator Trigger3() { yield break; }
    protected virtual IEnumerator Trigger4() { yield break; }
    protected virtual IEnumerator Trigger5() { yield break; }
    protected virtual IEnumerator Trigger6() { yield break; }
}