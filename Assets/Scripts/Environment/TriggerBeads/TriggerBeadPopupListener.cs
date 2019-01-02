using UnityEngine;
using System.Collections;

/*
 * A TriggerBead that opens a single message over the HUD when triggered,
 *      then waits for the player to do some action before displaying the rest of the  message.
 */
public class TriggerBeadPopupListener : TriggerBeadPopup {

    public enum Action { MoveWASD, StartBurningIronSteel, SelectDown };

    public Action[] actions;
    [HideInInspector]
    public string[] messages;

    private int index = 0;
    private Coroutine activeCoroutine;

    private void Awake() {
        messages = new string[actions.Length];
    }

    protected override void Trigger() {
        base.Trigger();
        // Await for player action
        activeCoroutine = StartCoroutine(WaitForAction());
    }

    IEnumerator WaitForAction() {
        switch (actions[index]) {
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
        }

        HUD.MessageOverlayController.Text.text = HUD.MessageOverlayController.Text.text + "\n\n" + messages[index];
        index++;
        if (index < messages.Length)
            activeCoroutine = StartCoroutine(WaitForAction());
        else {
            Destroy(transform.parent.gameObject);
            activeCoroutine = null;
        }
    }

    public void StopActionCoroutine() {
        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);
    }
}
