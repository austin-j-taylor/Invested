using UnityEngine;
using UnityEngine.UI;
using static TextCodes;

/*
 * Displays a toggleable overlay for the HUD with several instructions for how to play.
 */
public class HelpOverlayController : MonoBehaviour {

    public Text HelpTextLeft { get; private set; }
    public Text HelpTextRight { get; private set; }
    
    public bool IsOpen {
        get {
            return SettingsMenu.settingsData.helpOverlay == 1;
        }
        private set {
            SettingsMenu.settingsData.helpOverlay = value ? 1 : 0;
        }
    }

    void Awake() {
        HelpTextLeft = transform.Find("HelpTextLeft").GetComponent<Text>();
        HelpTextRight = transform.Find("HelpTextRight").GetComponent<Text>();
        IsOpen = false;
        //SetVisible(IsOpen);
        UpdateText();
    }

    public void UpdateText() {
        HelpTextLeft.text = KeyHelp + " to toggle the " + HelpOverlay + ".\n\n" +
            KeyLook + " to look.\n" +
                KeyMove + " to move" + ((SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) ? ".\n" : " (" + KeyWalk + " to move slowly).\n") +
                KeyJump + " to jump.\n\n" +
            KeyStartBurning + "\n\t\tto start burning and select " + Push_Pull_targets + ".\n" +
            KeyPullOrPush + " to " + Pull_or_Push + ".\n\n" +
            "While holding " + KeyNegate +
            ":\n\t\t• " + s_Press_ + KeySelect + " while looking at a " + Pull_target +
            " to deselect it.\n\t\t• " + s_Tap_ + KeySelect + " while not looking at a " + Pull_target + " to deselect your oldest " + Pull_target +
            ".\n\t\tLikewise for " + KeySelectAlternate + " and " + Push_targets + ".";
        HelpTextRight.text =
            KeyPushPullStrength + " to change " + Push_Pull + " " + BurnPercentage + "\n" +
            KeyNumberOfTargets + " to change your " + Gray("max number of " + Push_Pull_targets + ".\n") +
            KeySwap + " to swap your " + Push_targets + " and " + Pull_targets + ".\n" +
            KeyStopBurning + " to stop burning " + Gray("Iron and Steel") + ".\n\n" +
            KeyThrow + " to toss a coin.\n" +
            KeyDrop + " to drop a coin at your feet.\n\t\t• " + KeyDropDirection + " while dropping a coin to toss the coin away from that direction.\n\n" +
            KeySprint + " to burn " + Pewter + " for " + Sprinting + " and " + PewterJumping + "."


            ;
    }

    // Not called by Button
    public void Toggle() {
        SetVisible(!IsOpen);
        SettingsMenu.RefreshSettingHelpOverlay();
    }

    private void SetVisible(bool open) {
        IsOpen = open;
        HelpTextLeft.gameObject.SetActive(open);
        HelpTextRight.gameObject.SetActive(open);
    }

    public void Enable() {
        SetVisible(true);
    }

    public void Disable() {
        SetVisible(false);
    }
}