using UnityEngine;
using System.Collections;

/*
 * A TriggerBead that opens a single message over the HUD when triggered,
 *      then waits for the player to execute some action(s) before displaying the rest of the message.
 */
public class TriggerBeadPopupListener : TriggerBeadPopup {

    public enum Action { MoveWASD, StartBurningIronSteel, SelectDown, PushPull };

    public Action[] actions;
    public bool[] clearAfter;
    
    private int coroutinePosition = 0;

    protected override void Trigger() {
        base.Trigger();
        StartCoroutine(WaitForAction());
        // Await for player action
    }
    
    public override string GetText() {
        int clearIndex = 0;
        if (clearAfter != null) {
            for (int i = 0; i < clearAfter.Length; i++) {
                if (clearAfter[i]) {
                    clearIndex = i + 1;
                }
            }
        }
        string text = "";
        for (int i = clearIndex; i <= coroutinePosition; i++) {
            text += HUD.MessageOverlayController.TriggerBeadMessages[section][i] + "\n\n";
        }
        return text;
    }

    IEnumerator WaitForAction() {
        switch (actions[coroutinePosition]) {
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
                    bool selected = false;
                    bool selectedAlternate = false;
                    while (!selected || !selectedAlternate) {
                        selected = selected || Keybinds.SelectDown();
                        selectedAlternate = selectedAlternate || Keybinds.SelectAlternateDown();
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
        if (HUD.MessageOverlayController.CurrentPopup == this) {
            coroutinePosition++;
            HUD.MessageOverlayController.MessageText.text = GetText();
            if (coroutinePosition < HUD.MessageOverlayController.TriggerBeadMessages[section].Count - 1) {
                StartCoroutine(WaitForAction());
            }
        }
    }
}
