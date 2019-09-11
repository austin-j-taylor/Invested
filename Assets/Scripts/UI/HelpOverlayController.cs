using UnityEngine;
using UnityEngine.UI;
using static TextCodes;

/*
 * Displays a toggleable overlay for the HUD with several instructions for how to play.
 */
public class HelpOverlayController : MonoBehaviour {

    public enum LockedState { Unlocked, Locked0, Locked1 }

    public LockedState lockedState { get; private set; }

    private Text HelpTextLeft { get; set; }
    private Text HelpTextRight { get; set; }

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
    }

    public void Clear() {
        SetLockedState(LockedState.Unlocked);
    }

    public void UpdateText() {
        HelpTextLeft.text = KeyHelp + " to toggle the " + HelpOverlay + ".\n\n" +
            KeyLook + " to look.\n"
        ;
        Debug.Log(lockedState);
        switch (lockedState) {
            case LockedState.Locked0:
                HelpTextLeft.text += KeyMove + " to move.\n";
                HelpTextRight.text = string.Empty;
                break;
            case LockedState.Unlocked: // fall through
            case LockedState.Locked1:
                HelpTextLeft.text += KeyMove + " to move.\n\t\t• " + KeyWalk + " to move slowly and anchor yourself.\n";

                HelpTextRight.text =
                    KeyPushPullStrength + " to change " + Push_Pull + " " + BurnPercentage + "\n" +
                    KeyNumberOfTargets + " to change your " + Gray("max number of " + Push_Pull_targets + ".\n") +
                    KeySwap + " to swap your " + Push_targets + " and " + Pull_targets + ".\n" +
                    KeyStopBurning + " to stop burning " + Gray("Iron and Steel") +
                    (SettingsMenu.settingsData.controlScheme == SettingsData.MKQE || SettingsMenu.settingsData.controlScheme == SettingsData.MKEQ ?
                        "\n\t(Your keyboard may not support that last option.)\n\n" :
                        ".\n\n"
                    ) +
                    KeyThrow + " to toss a " + O_Coin + ".\n" +
                    KeyDrop + " to drop a " + O_Coin + " at your feet.\n\t\t• " + KeyDropDirection + " while dropping a " + O_Coin + " to toss the coin away from that direction.\n\n" +
                    KeySprint + " to burn " + Pewter + " for " + Sprinting + " and " + PewterJumping + ".\n\n"
                ;
                break;
        }


        HelpTextLeft.text += KeyJump + " to jump.\n\n" +
            KeyStartBurning + " to start burning " + Iron + "/" + Steel + ".\n" +
            s_Press_ + _KeySelect + " to select " + Pull_targets + " and " + _KeySelectAlternate + " to select " + Push_targets + ".\n" +
            s_Hold_ + _KeyPull + " to " + Pull + " and " + _KeyPush + " to " + Push + ".\n\n" +
            "While holding " + KeyNegate +
            ":\n\t\t• " + s_Press_ + _KeySelect + " to deselect a " + Pull_target +
            ".\n\t\t• " + s_Press_ + _KeySelectAlternate + " to deselect a " + Push_target + "."
        ;

        if (lockedState == LockedState.Unlocked) {
            HelpTextRight.text += KeyCoinshotMode + " to toggle " + CoinshotMode
                + ".\n\t\t• While in " + CoinshotMode + ", " + KeyCoinshotThrow + " to throw coins.\n" +
                KeyZincTime + " to activate " + ZincTime + "."
            ;
        }
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

    // Sets how many options are visible in the help overlay
    public void SetLockedState(LockedState newState) {
        lockedState = newState;
        UpdateText();
    }
}