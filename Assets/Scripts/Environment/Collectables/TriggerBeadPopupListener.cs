﻿//using UnityEngine;
//using System.Collections;

///*
// * A TriggerBead that opens a single message over the HUD when triggered,
// *      then waits for the player to execute some action(s) before displaying the rest of the message.
// */
//public class TriggerBeadPopupListener : TriggerBeadPopup {

//    public enum Action {
//        MoveWASD, StartBurningIronSteel, SelectSteel, PushPull,
//        Deselect, Help, ChangeNumberOfTargets, ChangeBurnPercentage, StopBurningIronSteel,
//        CollectCoin, ThrowCoin, ThrowCoinDown, SelectIron, BurnPewter
//    };

//    public Action[] actions;
//    public bool[] clearAfter;

//    [SerializeField]
//    private TriggerBead nextBead = null;
//    [SerializeField]
//    private bool activeAtStart = true;
//    private int coroutinePosition = 0;

//    private void Start() {
//        if (!activeAtStart)
//            transform.parent.gameObject.SetActive(false);
//    }

//    protected override void Trigger() {
//        base.Trigger();
//        StartCoroutine(WaitForAction());
//        // Await for player action
//    }

//    public override string GetText() {
//        int clearIndex = 0;
//        if (clearAfter != null) {
//            for (int i = 0; i < clearAfter.Length; i++) {
//                if (clearAfter[i]) {
//                    clearIndex = i + 1;
//                }
//            }
//        }
//        string text = "";
//        if (coroutinePosition < clearIndex) {
//            for (int i = 1; i <= coroutinePosition + 1; i++) { // Skip Header string
//                text += HUD.MessageOverlayController.TriggerBeadMessages[section][i] + "\n\n";
//            }
//        } else {
//            for (int i = 1 + clearIndex; i <= coroutinePosition + 1; i++) { // Skip Header string
//                text += HUD.MessageOverlayController.TriggerBeadMessages[section][i] + "\n\n";
//            }
//        }

//        return text;
//    }

//    IEnumerator WaitForAction() {
//        if (coroutinePosition < actions.Length) {
//            switch (actions[coroutinePosition]) {
//                case Action.MoveWASD: {
//                        while (Player.PlayerInstance.GetComponent<Rigidbody>().velocity == Vector3.zero) {
//                            yield return null;
//                        }
//                        break;
//                    }
//                case Action.StartBurningIronSteel: {
//                        while (!Prima.PrimaInstance.ActorIronSteel.IsBurning) {
//                            yield return null;
//                        }
//                        break;
//                    }
//                case Action.SelectSteel: {
//                        while (!Keybinds.SelectAlternateDown()) {
//                            yield return null;
//                        }
//                        break;
//                    }
//                case Action.PushPull: {
//                        bool pulled = false;
//                        bool pushed = false;
//                        while (!pulled || !pushed) {
//                            pulled = pulled || Keybinds.IronPulling();
//                            pushed = pushed || Keybinds.SteelPushing();
//                            yield return null;
//                        }
//                        break;
//                    }
//                case Action.Deselect: {
//                        bool select = false;
//                        bool alternate = false;
//                        while (!select || !alternate) {
//                            select = select || (Keybinds.Negate() && Keybinds.Select());
//                            alternate = alternate || (Keybinds.Negate() && Keybinds.SelectAlternate());
//                            yield return null;
//                        }
//                        break;
//                    }
//                case Action.Help: {
//                        while (!HUD.HelpOverlayController.IsOpen) {
//                            yield return null;
//                        }
//                        // Close Message
//                        Close();
//                        break;
//                    }
//                case Action.ChangeNumberOfTargets: {
//                        while (Prima.PrimaInstance.ActorIronSteel.PullTargets.Size == 1) {
//                            yield return null;
//                        }
//                        break;
//                    }
//                case Action.ChangeBurnPercentage: {
//                        while (Prima.PrimaInstance.ActorIronSteel.IronBurnPercentageTarget > .99f && Prima.PrimaInstance.ActorIronSteel.SteelBurnPercentageTarget > .99f) {
//                            yield return null;
//                        }
//                        break;
//                    }
//                case Action.StopBurningIronSteel: {
//                        while (Prima.PrimaInstance.ActorIronSteel.IsBurning) {
//                            yield return null;
//                        }
//                        Close();
//                        break;
//                    }
//                case Action.CollectCoin: {
//                        while (Prima.PrimaInstance.CoinHand.Pouch.Count == 0) {
//                            yield return null;
//                        }
//                        break;
//                    }
//                case Action.ThrowCoin: {
//                        while (!Keybinds.WithdrawCoinDown()) {
//                            yield return null;
//                        }
//                        break;
//                    }
//                case Action.ThrowCoinDown: {
//                        while (!Keybinds.WithdrawCoinDown() || !Keybinds.Jump()) {
//                            yield return null;
//                        }

//                        break;
//                    }
//                case Action.SelectIron: {
//                        while (!Keybinds.SelectDown()) {
//                            yield return null;
//                        }
//                        break;
//                    }
//                case Action.BurnPewter: {
//                        while (!Prima.PlayerPewter.IsSprinting) {
//                            yield return null;
//                        }
//                        break;
//                    }
//            }

//            // When one TriggerBeadPopup is entered, make sure no other TriggerBead listeners are still running to update the MessageOverlay later on.
//            if (HUD.MessageOverlayController.CurrentPopup == this) {
//                coroutinePosition++;
//                HUD.MessageOverlayController.MessageText.text = GetText();
//                HUD.MessageOverlayController.HeaderText.text = GetHeader();
//                if (coroutinePosition < HUD.MessageOverlayController.TriggerBeadMessages[section].Count - 2 || coroutinePosition < actions.Length) {
//                    StartCoroutine(WaitForAction());
//                    yield break;
//                }
//            }
//        }
//        // End of this bead's actions
//        if (nextBead) {
//            nextBead.transform.parent.gameObject.SetActive(true);
//        }
//    }
//}
