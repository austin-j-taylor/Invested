//using UnityEngine;
//using System.Collections;

///*
// * A TriggerBead that opens a single message over the HUD when triggered,
// *      then destroys itself.
// * The message closes after a period of time.
// */
//public class TriggerBeadPopupMessage : TriggerBeadPopup {

//    public enum Action { Default, ZincTime };

//    [SerializeField]
//    private Action action = Action.ZincTime;

//    protected override void Trigger() {
//        base.Trigger();

//        switch (action) {
//            case Action.Default: {
//                    // fade away after a time
//                    StartCoroutine(Fade(20));

//                    break;
//                }
//            case Action.ZincTime: {
//                    // fade away after a time
//                    StartCoroutine(Fade(30));

//                    break;
//                }
//        }
//    }

//    private IEnumerator Fade(float time) {
//        yield return new WaitForSeconds(time);
//        Close();
//    }
//}
