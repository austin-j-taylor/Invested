using UnityEngine;
using UnityEngine.UI;
using System.Text;
using static TextCodes;

/*
 * Displays a toggleable overlay for the HUD with several instructions for how to play.
 */
public class HelpOverlayController : MonoBehaviour {

    // Used to update the overlay when a change occurs
    private bool last_IronEnabled = false, last_SteelEnabled = false, last_ControlWheel = false, last_PewterEnabled = false, last_Zinc = false, last_Coins;

    private Text HelpTextLeft { get; set; }

    private enum State { Closed, Simple, Verbose}
    private State currentState;

    public bool IsOpen {
        get {
            return currentState != State.Closed;
        }
    }
    private bool Verbose => currentState == State.Verbose;

    void Awake() {
        HelpTextLeft = transform.Find("HelpTextLeft").GetComponent<Text>();
        currentState = State.Closed;
    }

    public void Clear() {

    }
    private void Update() {
        if(IsOpen) {
            // Update the text if a change in players' abilites has occured
            if(last_SteelEnabled != Player.PlayerIronSteel.SteelReserve.IsEnabled) {
                last_SteelEnabled = Player.PlayerIronSteel.SteelReserve.IsEnabled;
                UpdateText();
            }
            if(last_IronEnabled != Player.PlayerIronSteel.IronReserve.IsEnabled) {
                last_IronEnabled = Player.PlayerIronSteel.IronReserve.IsEnabled;
                UpdateText();
            }
            if(last_ControlWheel != !HUD.ControlWheelController.IsLocked()) {
                last_ControlWheel = !HUD.ControlWheelController.IsLocked();
                UpdateText();
            }
            if(last_PewterEnabled != Player.PlayerPewter.PewterReserve.IsEnabled) {
                last_PewterEnabled = Player.PlayerPewter.PewterReserve.IsEnabled;
                UpdateText();
            }
            if(last_Zinc != Player.CanControlZinc) {
                last_Zinc = Player.CanControlZinc;
                UpdateText();
            }
            if(last_Coins != Player.CanThrowCoins && Player.PlayerInstance.CoinHand.Pouch.Count > 0) {
                last_Coins = Player.CanThrowCoins && Player.PlayerInstance.CoinHand.Pouch.Count > 0;
                UpdateText();
            }
        }
    }

    // Update the text in the help overlay to reflect verbosity, current abilities, and current mode (e.g. Bubble)
    public void UpdateText() {
        if(currentState == State.Closed) {
            HelpTextLeft.gameObject.SetActive(false);
            return;
        }
        HelpTextLeft.gameObject.SetActive(true);
        StringBuilder builder = new StringBuilder();
        if(Verbose) {
            builder.AppendLine(KeyHelpAbridged + ": toggle this overlay (verbose)\n");
        } else {
            builder.AppendLine(KeyHelpAbridged + ": toggle this overlay (simple)\n");
        }
        builder.AppendLine(
            WASD + ": Move\n" +
            Mouse + ": Look"
        ); 
        if(last_SteelEnabled) {
            builder.AppendLine(
                _KeyPullPushAbridged + ": " + Pull_Push + '\n' +
                " • " + _KeyMarkAbridged + ": " + Mark + " for " + Pulling + '/' + Pushing
            );
            if(Verbose) {
                builder.AppendLine(" • " + KeyMultiMark + " + " + _KeyMarkAbridged + ": " + Mark + " on multiple targets");
            }
            builder.AppendLine(
                " • " + KeyPushPullStrength + ": " + Push_Pull + " strength"
            );
        } else {
            if (last_IronEnabled) {
                builder.AppendLine(
                    _KeyPullAbridged + ": " + Pull + '\n' +
                    " • " + _KeySelectAbridged + ": " + Mark + " for " + Pulling
                );
                if (Verbose) {
                    builder.AppendLine("• " + KeyMultiMark + " + " + _KeySelectAbridged + ": " + Mark + " on multiple targets");
                }
                builder.AppendLine(
                    " • " + KeyPushPullStrength + ": " + Pull + " strength"
                );
            }
        }
        if(last_ControlWheel) {
            builder.AppendLine(
                KeyControlWheel + ": " + ControlWheel
            );
            if(Verbose && SettingsMenu.settingsData.controlScheme != SettingsData.Gamepad) {
                builder.AppendLine(" • 1/2/3/4/C/X/Z: " + ControlWheel + " hotkeys");
            }
        }

        if(last_PewterEnabled) {
            builder.AppendLine(
                KeySprint + ": " + Sprint + '\n' + 
                KeyWalk + ": " + Anchor
            );
        }

        if(last_Zinc) {
            builder.AppendLine(KeyZincTime + ": " + Zinc);
        }

        if(last_Coins) {
            builder.AppendLine(KeyThrowAbridged + ": Throw " + O_Coin);
            if(Verbose) {
                if(!SettingsMenu.settingsData.UsingGamepad) {
                    builder.AppendLine(" • " + Shift + " + " + KeyThrowAbridged + ": Mark and Throw " + O_Coin);
                }
                builder.AppendLine(" • " + KeyJump + " + " + KeyThrowAbridged + ": Throw " + O_Coin + " downwards" +
                    "\n • " + KeyWalk + " + " + KeyThrowAbridged + ": Toss without " + Pushing);
            }
        }

        HelpTextLeft.text = builder.ToString();
    }

    // Called by pressing the H or F1 key
    public void Toggle() {
        switch(currentState) {
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