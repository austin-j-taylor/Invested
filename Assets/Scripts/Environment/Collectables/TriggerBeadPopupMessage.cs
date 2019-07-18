using UnityEngine;
using System.Collections;

/*
 * A TriggerBead that opens a single message over the HUD when triggered,
 *      then destroys itself.
 * The message closes after a period of time.
 */
public class TriggerBeadPopupMessage : TriggerBeadPopup {

    private const int fadeTime = 20;

    public enum Action { Default, ZincTime };

    [SerializeField]
    private Action action = Action.ZincTime;

    protected override void Trigger() {
        base.Trigger();

        switch(action) {
            case Action.Default: {
                    // fade away after a time
                    StartCoroutine(Fade());
                    
                    break;
                }
            case Action.ZincTime: {
                    // fade away after a time
                    StartCoroutine(Fade());
                    FlagsController.HelpOverlayFuller = true;

                    break;
                }
        }
    }

    private IEnumerator Fade() {
        yield return new WaitForSeconds(fadeTime);
        Close();
    }
}
