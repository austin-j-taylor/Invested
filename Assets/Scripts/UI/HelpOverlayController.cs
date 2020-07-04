using UnityEngine;
using UnityEngine.UI;
using System.Text;
using static TextCodes;
using static PlayerPullPushController;

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

    private Text HelpTextLeft { get; set; }
    // Used to update the overlay when a change occurs
    private bool last_IronEnabled = false, last_SteelEnabled = false, last_ControlWheel = false, last_PewterEnabled = false, last_Zinc = false, last_Coins;
    private ControlMode last_Mode = ControlMode.Manual;
    private State currentState;

    void Awake() {
        HelpTextLeft = transform.Find("HelpTextLeft").GetComponent<Text>();
        currentState = State.Closed;
    }

    public void Clear() { }

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
            if (last_Coins != Player.CanThrowCoins && Player.PlayerInstance.CoinHand.Pouch.Count > 0) {
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
            builder.AppendLine(HowToHelpAbridged + ": toggle this overlay (verbose)\n");
        } else {
            builder.AppendLine(HowToHelpAbridged + ": toggle this overlay (simple)\n");
        }
        builder.AppendLine(
            WASD + ": Move\n" +
            Mouse + ": Look"
        );

        switch (last_Mode) {
            case ControlMode.Manual:
                if (last_SteelEnabled) {
                    builder.AppendLine(
                        KeyPullPushAbridged + ": " + Pull_Push + '\n' +
                        " • " + KeyMarkAbridged + ": " + Mark + " for " + Pulling + '/' + Pushing
                    );
                    if (Verbose) {
                        builder.AppendLine(" • " + KeyMultiMark + " + " + KeyMarkAbridged + ": " + Mark + " on multiple targets");
                    }
                    builder.AppendLine(
                        " • " + KeyPushPullStrength + ": " + Push_Pull + " strength"
                    );
                } else {
                    if (last_IronEnabled) {
                        builder.AppendLine(
                            KeyPullAbridged + ": " + Pull + '\n' +
                            " • " + KeyMark_PullAbridged + ": " + Mark + " for " + Pulling
                        );
                        if (Verbose) {
                            builder.AppendLine(" • " + KeyMultiMark + " + " + KeyMark_PullAbridged + ": " + Mark + " on multiple targets");
                        }
                        builder.AppendLine(
                            " • " + KeyPushPullStrength + ": " + Pull + " strength"
                        );
                    }
                }
                break;
            case ControlMode.Coinshot: // Ignore SteelEnabled and IronEnabled because you should never be in Coinshot without Steel
                builder.AppendLine(
                    KeyPullAbridged + ": Throw and " + Push + " " + O_Coin
                );
                if (Verbose) {
                    builder.AppendLine(" • " + s_Hold_ + KeyMultiMark + ": " + Mark + " when thrown");
                }
                builder.AppendLine(
                    KeyPushAbridged + ": " + Push + '\n' +
                    " • " + KeyMarkAbridged + ": " + Mark + " for " + Pulling + '/' + Pushing
                );
                if (Verbose) {
                    builder.AppendLine(" • " + KeyMultiMark + " + " + KeyMarkAbridged + ": " + Mark + " on multiple targets");
                }
                builder.AppendLine(
                    " • " + KeyPushPullStrength + ": " + Push_Pull + " strength"
                );
                break;
            case ControlMode.Area:
                if (last_SteelEnabled) {
                    builder.AppendLine(
                        KeyPullPushAbridged + ": " + Pull_Push + '\n' +
                        " • " + KeyMarkAbridged + ": " + Mark + " for " + Pulling + '/' + Pushing
                    );
                    if (Verbose) {
                        builder.AppendLine(" • " + KeyMultiMark + " + " + KeyMarkAbridged + ": " + Mark + " on multiple targets");
                    }
                    builder.AppendLine(
                        " • " + KeyPushPullStrength + ": " + Push_Pull + " strength\n" +
                        " • " + KeyRadiusAbridged + ": size of area"
                    );
                } else {
                    if (last_IronEnabled) {
                        builder.AppendLine(
                            KeyPullAbridged + ": " + Pull + '\n' +
                            " • " + KeyMark_PullAbridged + ": " + Mark + " for " + Pulling
                        );
                        if (Verbose) {
                            builder.AppendLine(" • " + KeyMultiMark + " + " + KeyMark_PullAbridged + ": " + Mark + " on multiple targets");
                        }
                        builder.AppendLine(
                            " • " + KeyPushPullStrength + ": " + Pull + " strength\n" +
                            " • " + KeyRadiusAbridged + ": size of area"
                        );
                    }
                }
                break;
            case ControlMode.Bubble:
                if (last_SteelEnabled) {
                    builder.AppendLine(
                        KeyPullPushAbridged + ": " + Pull_Push + '\n' +
                        " • " + KeyMarkAbridged + ": Toggle bubble"
                    );
                    builder.AppendLine(
                        " • " + KeyPushPullStrength + ": " + Push_Pull + " strength\n" +
                        " • " + KeyRadiusAbridged + ": size of bubble"
                    );
                } else {
                    if (last_IronEnabled) {
                        builder.AppendLine(
                            KeyPullAbridged + ": " + Pull + '\n' +
                            " • " + KeyMarkAbridged + ": toggle bubble"
                        );
                        builder.AppendLine(
                            " • " + KeyPushPullStrength + ": " + Pull + " strength\n" +
                            " • " + KeyRadiusAbridged + ": size of bubble"
                        );
                    }
                }
                break;
        }

        if (last_ControlWheel) {
            builder.AppendLine(
                KeyControlWheel + ": " + ControlWheel
            );
            if (Verbose && SettingsMenu.settingsGameplay.controlScheme != JSONSettings_Gameplay.Gamepad) {
                builder.AppendLine(" • 1/2/3/4/C/X/Z: " + ControlWheel + " hotkeys");
            }
        }

        if (last_PewterEnabled) {
            builder.AppendLine(
                KeySprint + ": " + Sprint + '\n' +
                KeyAnchor + ": " + Anchor
            );
        }

        if (last_Zinc) {
            builder.AppendLine(KeyZincTime + ": " + Zinc);
        }

        if (last_Coins) {
            builder.AppendLine(KeyThrow + ": Throw " + O_Coin);
            if (Verbose) {
                //if(!SettingsMenu.settingsData.UsingGamepad) {
                //    builder.AppendLine(" • " + Shift + " + " + KeyThrowAbridged + ": Mark and Throw " + O_Coin);
                //}
                builder.AppendLine(" • " + KeyJump + " + " + KeyThrow + ": Throw " + O_Coin + " downwards, weighted by" + KeyMove +
                    "\n • " + KeyAnchor + " + " + KeyThrow + ": Toss without " + Pushing);
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
        SettingsMenu.RefreshSettingHelpOverlay((int)currentState);
    }
    
    public void SetState(int intState) {
        State newState = (State)intState;
        if(newState != currentState) {
            switch (newState) {
                case State.Closed:
                    Disable();
                    break;
                case State.Simple:
                    EnableSimple();
                    break;
                case State.Verbose:
                    EnableVerbose();
                    break;
            }
        }
    }

    public void EnableSimple() {
        currentState = State.Simple;
        UpdateText();
    }
    public void EnableVerbose() {
        currentState = State.Verbose;
        UpdateText();
    }
    public void Disable() {
        currentState = State.Closed;
        UpdateText();
    }
}