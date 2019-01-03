using UnityEngine;
using System.Collections;

/*
 * A TriggerBead that opens a single message over the HUD when triggered,
 *      then waits for the player to do some action before displaying the rest of the  message.
 */
public class TriggerBeadPopupListener : TriggerBeadPopup {

    public enum Action { MoveWASD, StartBurningIronSteel, SelectDown, PushPull };

    public Action[] actions;
    public bool[] clearAfter;
    [HideInInspector]
    public string[] messages;
    
    private void Start() {
        messages = new string[actions.Length];
    }

    protected override void Trigger() {
        base.Trigger();
        transform.parent.gameObject.AddComponent<ActionListener>();
        // Await for player action
        Destroy(gameObject);
    }

    private class ActionListener : MonoBehaviour {

        TriggerBeadPopupListener parent;
        private Coroutine activeCoroutine;
        private int index = 0;

        public void Awake() {
            parent = transform.GetChild(0).GetComponent<TriggerBeadPopupListener>();
            activeCoroutine = StartCoroutine(WaitForAction());
            parent.overhead.currentListenerCoroutine = activeCoroutine;
        }

        IEnumerator WaitForAction() {
            
            switch (parent.actions[index]) {
                case Action.MoveWASD: {
                        while (Player.PlayerInstance.GetComponent<Rigidbody>().velocity == Vector3.zero) {
                            yield return null;
                        }
                        break;
                    }
                case Action.StartBurningIronSteel: {
                        while (!Player.PlayerIronSteel.IsBurningIronSteel) {
                            yield return null;
                        }
                        break;
                    }
                case Action.SelectDown: {
                        while (!Keybinds.SelectDown() && !Keybinds.SelectAlternateDown()) {
                            yield return null;
                        }
                        break;
                    }
                case Action.PushPull: {
                        while (!Keybinds.IronPulling() && !Keybinds.SteelPushing()) {
                            yield return null;
                        }
                        break;
                    }
            }

            // When one TriggerBeadPopup is entered, make sure no other TriggerBead listeners are still running to update the MessageOverlay later on.
            if (parent.overhead.currentListenerCoroutine == activeCoroutine) {
                if (parent.clearAfter != null && index < parent.clearAfter.Length && parent.clearAfter[index]) {
                    HUD.MessageOverlayController.Text.text = parent.messages[index];
                } else {
                    HUD.MessageOverlayController.Text.text = HUD.MessageOverlayController.Text.text + "\n\n" + parent.messages[index];
                }
                index++;
                if (index < parent.messages.Length) {
                    activeCoroutine = StartCoroutine(WaitForAction());
                    parent.overhead.currentListenerCoroutine = activeCoroutine;
                }
            } else { // If player has collected another TriggerBead listener while this one was running
                Destroy(gameObject);
            }
        }
    }
}
