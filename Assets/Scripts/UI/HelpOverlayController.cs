using UnityEngine;
using UnityEngine.UI;
using System.Text;
using static TextCodes;
using static PrimaPullPushController;
using TMPro;

/// <summary>
/// Controls the HUD element that displays help information to the player in the top-left corner
///     Simple: show the fundamental controls
///     Verbose: show all controls
///     Disabled: disabled
/// </summary>
public class HelpOverlayController : MonoBehaviour {

    private enum State { Closed, Simple, Verbose }

    public bool IsOpen => currentState != State.Closed;
    private bool Verbose => currentState == State.Verbose;

    private TextMeshProUGUI HelpTextLeft { get; set; }
    // Used to update the overlay when a change occurs
    private bool last_IronEnabled = false, last_SteelEnabled = false, last_ControlWheel = false, last_PewterEnabled = false, last_Zinc = false, last_Coins = false;
    private ControlMode last_Mode = ControlMode.Manual;
    private State currentState;

    void Awake() {
        HelpTextLeft = transform.Find("HelpTextLeft").GetComponent<TextMeshProUGUI>();
        currentState = State.Closed;
    }

    public void Clear() {
        UpdateText();
    }

    #region update
    private void Update() {
        if (IsOpen) {
            // Update the text if a change in players' abilites has occured
            if (last_SteelEnabled != Player.PlayerIronSteel.SteelReserve.IsEnabled) {
                last_SteelEnabled = Player.PlayerIronSteel.SteelReserve.IsEnabled;
                UpdateText();
            }
            if (last_IronEnabled != Player.PlayerIronSteel.IronReserve.IsEnabled) {
                last_IronEnabled = Player.PlayerIronSteel.IronReserve.IsEnabled;
                UpdateText();
            }
            if (last_ControlWheel != !HUD.ControlWheelController.IsLocked()) {
                last_ControlWheel = !HUD.ControlWheelController.IsLocked();
                UpdateText();
            }
            if (last_PewterEnabled != Player.PlayerPewter.PewterReserve.IsEnabled) {
                last_PewterEnabled = Player.PlayerPewter.PewterReserve.IsEnabled;
                UpdateText();
            }
            if (last_Zinc != Player.CanControlZinc) {
                last_Zinc = Player.CanControlZinc;
                UpdateText();
            }
            if (last_Coins != (Player.CanThrowCoins && Player.PlayerInstance.CoinHand.Pouch.Count > 0)) {
                last_Coins = Player.CanThrowCoins && Player.PlayerInstance.CoinHand.Pouch.Count > 0;
                UpdateText();
            }
            if (last_Mode != Player.PlayerIronSteel.Mode) {
                last_Mode = Player.PlayerIronSteel.Mode;
                UpdateText();
            }
        }
    }

    // Update the text in the help overlay to reflect verbosity, current abilities, and current mode (e.g. Bubble)
    public void UpdateText() {
        if (currentState == State.Closed) {
            HelpTextLeft.gameObject.SetActive(false);
            return;
        }
        HelpTextLeft.gameObject.SetActive(true);
        StringBuilder builder = new StringBuilder();
        if (Verbose) {
            builder.AppendLine(HowToHelpAbridged + " toggle this overlay (verbose)\n");
            builder.AppendLine(HowToPerspectiveAbridged + " change perspective");
            builder.AppendLine(HowToTextLog + " view text log");
        } else {
            builder.AppendLine(HowToHelpAbridged + " toggle this overlay (simple)\n");
        }
        builder.AppendLine(
            KeyMoveAbridged + " Move\n" +
            KeyLookAbridged + " Look\n" +
            KeyJump + " Jump"
        );

        switch (last_Mode) {
            case ControlMode.Manual:
                if (last_SteelEnabled) {
                    builder.AppendLine(
                        KeyPullPushAbridged + " " + Pull_Push + '\n' +
                        " • " + KeyMark + " " + Mark + " for " + Pulling + '/' + Pushing
                    );
                    if (Verbose) {
                        builder.AppendLine(" • " + KeyMultiMark + " " + Mark + " on multiple targets");
                    }
                    builder.AppendLine(
                        " • " + KeyPushPullStrength + " " + Push_Pull + " strength"
                    );
                } else {
                    if (last_IronEnabled) {
                        builder.AppendLine(
                            KeyPullAbridged + " " + Pull + '\n' +
                            " • " + KeyMark_Pull + " " + Mark + " for " + Pulling
                        );
                        if (Verbose) {
                            builder.AppendLine(" • " + KeyMultiMark_Pull + " " + Mark + " on multiple targets");
                        }
                        builder.AppendLine(
                            " • " + KeyPullStrength + " " + Pull + " strength"
                        );
                    }
                }
                break;
            case ControlMode.Coinshot: // Ignore SteelEnabled and IronEnabled because you should never be in Coinshot without Steel
                builder.AppendLine(
                    KeyPullAbridged + " Throw and " + Push + " " + O_Coin
                );
                if (Verbose) {
                    builder.AppendLine(" • " + s_Hold_ + KeyMultiMark + " " + Mark + " when thrown");
                }
                builder.AppendLine(
                    KeyPushAbridged + " " + Push + '\n' +
                    " • " + KeyMark + " " + Mark + " for " + Pulling + '/' + Pushing
                );
                if (Verbose) {
                    builder.AppendLine(" • " + KeyMultiMark + " " + Mark + " on multiple targets");
                }
                builder.AppendLine(
                    " • " + KeyPushPullStrength + " " + Push_Pull + " strength"
                );
                break;
            case ControlMode.Area:
                if (last_SteelEnabled) {
                    builder.AppendLine(
                        KeyPullPushAbridged + " " + Pull_Push + '\n' +
                        " • " + KeyMark + " " + Mark + " for " + Pulling + '/' + Pushing
                    );
                    if (Verbose) {
                        builder.AppendLine(" • " + KeyMultiMark + " " + Mark + " on multiple targets");
                    }
                    builder.AppendLine(
                        " • " + KeyPushPullStrength  + " " + Push_Pull + " strength\n" +
                        " • " + KeyRadiusAbridged + " size of area"
                    );
                } else {
                    if (last_IronEnabled) {
                        builder.AppendLine(
                            KeyPullAbridged + " " + Pull + '\n' +
                            " • " + KeyMark_Pull + " " + Mark + " for " + Pulling
                        );
                        if (Verbose) {
                            builder.AppendLine(" • " + KeyMultiMark_Pull + " " + Mark + " on multiple targets");
                        }
                        builder.AppendLine(
                            " • " + KeyPullStrength + " " + Pull + " strength\n" +
                            " • " + KeyRadiusAbridged + " size of area"
                        );
                    }
                }
                break;
            case ControlMode.Bubble:
                if (last_SteelEnabled) {
                    builder.AppendLine(
                        KeyPullPushAbridged + " " + Pull_Push + '\n' +
                        " • " + KeyMark + " Toggle bubble"
                    );
                    builder.AppendLine(
                        " • " + KeyPushPullStrength  + " " + Push_Pull + " strength\n" +
                        " • " + KeyRadiusAbridged + " size of bubble"
                    );
                } else {
                    if (last_IronEnabled) {
                        builder.AppendLine(
                            KeyPullAbridged + " " + Pull + '\n' +
                            " • " + KeyMark + " toggle bubble"
                        );
                        builder.AppendLine(
                            " • " + KeyPullStrength + " " + Pull + " strength\n" +
                            " • " + KeyRadiusAbridged + " size of bubble"
                        );
                    }
                }
                break;
        }

        if (last_ControlWheel) {
            builder.AppendLine(
                KeyControlWheel + " " + ControlWheel
            );
            if (Verbose && SettingsMenu.settingsGameplay.controlScheme != JSONSettings_Gameplay.Gamepad) {
                builder.AppendLine(" • " + KeyManual + KeyArea + KeyBubble + KeyBubblePolarity + KeyCoinshot + KeyThrowingMode + KeyDeselectAll + KeyStopBurning + " " + ControlWheel + " hotkeys");
            }
        }

        if (last_PewterEnabled) {
            builder.AppendLine(
                KeySprint + " " + Sprint + '\n' +
                KeyAnchor + " " + Anchor
            );
        }

        if (last_Zinc) {
            if(SettingsMenu.settingsGameplay.UsingGamepad)
                builder.AppendLine(KeyZincTime + " Toggle " + Zinc);
            else
                builder.AppendLine(KeyZincTime + " " + Zinc);
        }

        if (last_Coins) {
            builder.AppendLine(KeyThrow + " Throw " + O_Coin);
            if (Verbose) {
                //if(!SettingsMenu.settingsData.UsingGamepad) {
                //    builder.AppendLine(" • " + Shift + " + " + KeyThrowAbridged + " Mark and Throw " + O_Coin);
                //}
                builder.AppendLine(" • " + KeyJump + " + " + KeyThrow + " Throw " + O_Coin + " downwards, weighted by " + KeyMove +
                    "\n • " + KeyToss + " Toss without " + Pushing);
            }
        }

        HelpTextLeft.text = builder.ToString();
    }
    #endregion

    /// <summary>
    /// Toggles the help overlay between its simple, verbose, and disabled states
    /// </summary>
    public void Toggle() {
        switch (currentState) {
            case State.Closed:
                EnableSimple();
                break;
            case State.Simple:
                EnableVerbose();
                break;
            case State.Verbose:
                Disable();
                break;
        }
    }

    /// <summary>
    /// Sets the state to the state corresponding to intState. Should only be used by JSONSettings_Interface.
    /// </summary>
    /// <param name="intState">the State as an integer</param>
    public void SetState(int intState) {
        State newState = (State)intState;
        if (newState != currentState) {
            switch (newState) {
                case State.Closed:
                    Disable(false);
                    break;
                case State.Simple:
                    EnableSimple(false);
                    break;
                case State.Verbose:
                    EnableVerbose(false);
                    break;
            }
        }
    }

    private void EnableSimple(bool refreshSetting = true) {
        currentState = State.Simple;
        UpdateText();
        if(refreshSetting)
            SettingsMenu.RefreshSettingHelpOverlay((int)currentState);
    }
    private void EnableVerbose(bool refreshSetting = true) {
        currentState = State.Verbose;
        UpdateText();
        if (refreshSetting)
            SettingsMenu.RefreshSettingHelpOverlay((int)currentState);
    }
    private void Disable(bool refreshSetting = true) {
        currentState = State.Closed;
        UpdateText();
        if (refreshSetting)
            SettingsMenu.RefreshSettingHelpOverlay((int)currentState);
    }
}