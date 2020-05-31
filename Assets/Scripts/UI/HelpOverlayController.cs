using UnityEngine;
using UnityEngine.UI;
using System.Text;
using static TextCodes;

/*
 * Displays a toggleable overlay for the HUD with several instructions for how to play.
 */
public class HelpOverlayController : MonoBehaviour {

    private Text HelpTextLeft { get; set; }
    //private Text HelpTextRight { get; set; }

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
        //HelpTextRight = transform.Find("HelpTextRight").GetComponent<Text>();
        currentState = State.Closed;
    }

    public void Clear() {

    }

    // Update the text in the help overlay to reflect verbosity, current abilities, and current mode (e.g. Bubble)
    public void UpdateText() {
        if(currentState == State.Closed) {
            HelpTextLeft.gameObject.SetActive(false);
            return;
        }
        HelpTextLeft.gameObject.SetActive(true);
        StringBuilder builder = new StringBuilder();
        builder.AppendLine(
            KeyHelpAbridged + ": toggle this overlay\n" + 
            WASD + ": Move\n" +
            Mouse + ": Look"
        ); 
        if(Player.PlayerIronSteel.SteelReserve.IsEnabled) {
            builder.AppendLine(
                _KeyPullPushAbridged + ": " + Pull_Push + '\n' +
                " • " + _KeyMarkAbridged + ": " + Mark + " for " + Pulling + '/' + Pushing
            );
            if(Verbose) {
                builder.AppendLine(" • " + KeyMultiMark + " + " + _KeyMarkAbridged + ": " + Mark + " on multiple targets");
            }
        } else {
            builder.AppendLine(
                _KeyPullAbridged + ": " + Pull + '\n' +
                " • " + _KeySelectAbridged + ": " + Mark + " for " + Pulling
            );
            if (Verbose) {
                builder.AppendLine("• " + KeyMultiMark + " + " + _KeySelectAbridged + ": " + Mark + " on multiple targets");
            }
        }
        builder.AppendLine(
            " • " + KeyPushPullStrength + ": " + Push_Pull + " strength"
        );
        if(HUD.ControlWheelController.IsLocked()) {
            builder.AppendLine(
                KeyControlWheel + ": " + ControlWheel
            );
            if(Verbose && SettingsMenu.settingsData.controlScheme != SettingsData.Gamepad) {
                builder.AppendLine(" • 1/2/3/4/C/X/Z: " + ControlWheel + " hotkeys");
            }
        }

        if(Player.PlayerPewter.PewterReserve.IsEnabled) {
            builder.AppendLine(
                KeySprint + ": " + Sprint + '\n' + 
                KeyWalk + ": " + Anchor
            );
        }

        if(Player.CanControlZinc) {
            builder.AppendLine(KeyZincTime + ": " + Zinc);
        }

        if(Player.CanThrowCoins) {
            builder.AppendLine(KeyThrowAbridged + ": Throw " + O_Coin);
            if(Verbose) {
                if(!SettingsMenu.settingsData.UsingGamepad) {
                    builder.AppendLine(" • " + Shift + " + " + KeyThrowAbridged + ": Mark and Throw " + O_Coin);
                }
                builder.AppendLine(" • " + KeyJump + " + " + KeyThrowAbridged + ": Throw " + O_Coin + " downwards");
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