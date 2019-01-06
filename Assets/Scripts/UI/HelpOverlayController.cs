using UnityEngine;
using UnityEngine.UI;

/*
 * Displays a toggleable overlay for the HUD with several instructions for how to play.
 */
public class HelpOverlayController : MonoBehaviour {

    public Text HelpText { get; private set; }
    
    void Awake() {
        HelpText = GetComponentInChildren<Text>();
        UpdateText();
        HelpText.enabled = false;
    }

    public void UpdateText() {
        HelpText.text = TextCodes.KeyLook + " to look.\n" + TextCodes.KeyMove + " to move.\n" + TextCodes.KeyJump + " to jump.\n\n" +
                TextCodes.KeyStartBurning + "\nto start burning.\t\n\t" +
                TextCodes.s_Press_ + TextCodes.KeySelect + " or " + TextCodes.KeySelectAlternate + "\nto select a metal to be a " + TextCodes.Pull_or_Push_target + ".\t\n\n\t" +
                TextCodes.KeyPullOrPush + " to " + TextCodes.Pull_or_Push + ".\n";
    }
}