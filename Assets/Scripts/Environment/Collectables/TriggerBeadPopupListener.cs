using UnityEngine;
using System.Collections;

/*
 * A TriggerBead that opens a single message over the HUD when triggered,
 *      then waits for the player to execute some action(s) before displaying the rest of the message.
 */
public class TriggerBeadPopupListener : TriggerBeadPopup {

    public enum Action { MoveWASD, StartBurningIronSteel, SelectDown, PushPull, Deselect, Help, ChangeNumberOfTargets, ChangeBurnPercentage, StopBurningIronSteel, CollectCoin, ThrowCoin, ThrowCoinDown };

    public Action[] actions;
    public bool[] clearAfter;

    [SerializeField]
    private TriggerBead nextBead;
    [SerializeField]
    private bool activeAtStart = true;
    private int coroutinePosition = 0;

    private void Start() {
        if (!activeAtStart)
            transform.parent.gameObject.SetActive(false);
    }

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
        if (coroutinePosition < clearIndex) {
            for (int i = 1; i <= coroutinePosition + 1; i++) { // Skip Header string
                text += HUD.MessageOverlayController.TriggerBeadMessages[section][i] + "\n\n";
            }
        } else {
            for (int i = 1 + clearIndex; i <= coroutinePosition + 1; i++) { // Skip Header string
                text += HUD.MessageOverlayController.TriggerBeadMessages[section][i] + "\n\n";
            }
        }

        return text;
    }

    IEnumerator WaitForAction() {
        if (coroutinePosition < actions.Length) {
            switch (actions[coroutinePosition]) {
                case Action.MoveWASD: {
                        while (Player.PlayerInstance.GetComponent<Rigidbody>().velocity == Vector3.zero) {
                            yield return null;
                        }
                        break;
                    }
                case Action.StartBurningIronSteel: {
                        while (!Player.PlayerIronSteel.IsBurning) {
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
                        bool pulled = false;
                        bool pushed = false;
                        while (!pulled || !pushed) {
                            pulled = pulled || Keybinds.IronPulling();
                            pushed = pushed || Keybinds.SteelPushing();
                            yield return null;
                        }
                        break;
                    }
                case Action.Deselect: {
                        while (!(Keybinds.Negate() && Keybinds.Select()) && !(Keybinds.Negate() && Keybinds.SelectAlternate())) {
                            yield return null;
                        }
                        break;
                    }
                case Action.Help: {
                        while (!Keybinds.ToggleHelpOverlay()) {
                            yield return null;
                        }
                        // Close Message
                        Close();
                        break;
                    }
                case Action.ChangeNumberOfTargets: {
                        while (Player.PlayerIronSteel.PullTargets.Size == 1) {
                            yield return null;
                        }
                        break;
                    }
                case Action.ChangeBurnPercentage: {
                        while (Player.PlayerIronSteel.IronBurnRateTarget > .99f && Player.PlayerIronSteel.SteelBurnRateTarget > .99f) {
                            yield return null;
                        }
                        break;
                    }
                case Action.StopBurningIronSteel: {
                        while (Player.PlayerIronSteel.IsBurning) {
                            yield return null;
                        }
                        Close();
                        break;
                    }
                case Action.CollectCoin: {
                        while (Player.PlayerInstance.CoinHand.Pouch.Count == 0) {
                            yield return null;
                        }
                        break;
                    }
                case Action.ThrowCoin: {
                        while (!Keybinds.WithdrawCoinDown()) {
                            yield return null;
                        }
                        break;
                    }
                case Action.ThrowCoinDown: {
                        while (!Keybinds.WithdrawCoinDown() || !Keybinds.Jump()) {
                            yield return null;
                        }
                        break;
                    }
            }

            // When one TriggerBeadPopup is entered, make sure no other TriggerBead listeners are still running to update the MessageOverlay later on.
            if (HUD.MessageOverlayController.CurrentPopup == this) {
                coroutinePosition++;
                HUD.MessageOverlayController.MessageText.text = GetText();
                HUD.MessageOverlayController.HeaderText.text = GetHeader();
                if (coroutinePosition < HUD.MessageOverlayController.TriggerBeadMessages[section].Count - 2 || coroutinePosition < actions.Length) {
                    StartCoroutine(WaitForAction());
                    yield break;
                }
            }
        }
        // End of this bead's actions
        if (nextBead) {
            nextBead.transform.parent.gameObject.SetActive(true);
        }
    }
}
