using UnityEngine;
using UnityEngine.UI;

/*
 * Displays a toggleable overlay for the HUD with several instructions for how to play.
 */
public class HelpOverlayController : MonoBehaviour {

    public Text HelpTextLeft { get; private set; }
    public Text HelpTextRight { get; private set; }

    bool open;

    void Awake() {
        HelpTextLeft = transform.Find("HelpTextLeft").GetComponent<Text>();
        HelpTextRight = transform.Find("HelpTextRight").GetComponent<Text>();
        open = false;
        HelpTextLeft.enabled = false;
        HelpTextRight.enabled = false;
        UpdateText();
    }

    public void UpdateText() {
        HelpTextLeft.text = TextCodes.KeyLook + " to look.\n" + TextCodes.KeyMove + " to move.\n" + TextCodes.KeyJump + " to jump.\n\n" +
            TextCodes.KeyStartBurning + "\n\t\tto start burning and select " + TextCodes.Push_Pull_targets + ".\n" +
            TextCodes.KeyPullOrPush + " to " + TextCodes.Pull_or_Push + ".\n\n" +
            "While holding " + TextCodes.KeyNegate +
            ":\n\t\t• " + TextCodes.s_Press_ + TextCodes.KeySelect + " while looking at a " + TextCodes.Pull_target +
            " to deselect it.\n\t\t• " + TextCodes.s_Tap_ + TextCodes.KeySelect + " while not looking at a " + TextCodes.Pull_target + " to deselect your oldest " + TextCodes.Pull_target +
            ".\n\t\tLikewise for " + TextCodes.KeySelectAlternate + " and " + TextCodes.Push_targets + ".";
        HelpTextRight.text = TextCodes.KeyPushPullStrength + " to change " + TextCodes.Push_Pull + TextCodes.Gray(" strength") + "\n" +
            TextCodes.KeyNumberOfTargets + " to change your " + TextCodes.Gray("max number of " + TextCodes.Push_Pull_targets + ".\n") +
            TextCodes.KeySwap + " to swap your " + TextCodes.Push_targets + " and " + TextCodes.Pull_targets + ".\n" +
            TextCodes.KeyPassiveBurn + " to change which metal you passively burn.\n" +
            TextCodes.KeyStopBurning + " to stop burning " + TextCodes.Gray("Iron and Steel.");
    }

    public void Toggle() {
        open = !open;
        HelpTextLeft.enabled = open;
        HelpTextRight.enabled = open;
    }
}