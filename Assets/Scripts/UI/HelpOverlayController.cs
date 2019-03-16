using UnityEngine;
using UnityEngine.UI;
using static TextCodes;

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
        HelpTextLeft.text = KeyLook + " to look.\n" + KeyMove + " to move.\n" + KeyJump + " to jump.\n\n" +
            KeyStartBurning + "\n\t\tto start burning and select " + Push_Pull_targets + ".\n" +
            KeyPullOrPush + " to " + Pull_or_Push + ".\n\n" +
            "While holding " + KeyNegate +
            ":\n\t\t• " + s_Press_ + KeySelect + " while looking at a " + Pull_target +
            " to deselect it.\n\t\t• " + s_Tap_ + KeySelect + " while not looking at a " + Pull_target + " to deselect your oldest " + Pull_target +
            ".\n\t\tLikewise for " + KeySelectAlternate + " and " + Push_targets + ".";
        HelpTextRight.text = KeyPushPullStrength + " to change " + Push_Pull + " " + BurnPercentage + "\n" +
            KeyNumberOfTargets + " to change your " + Gray("max number of " + Push_Pull_targets + ".\n") +
            KeySwap + " to swap your " + Push_targets + " and " + Pull_targets + ".\n" +
            KeyPassiveBurn + " to change which metal you passively burn.\n" +
            KeyStopBurning + " to stop burning " + Gray("Iron and Steel.");
    }

    public void Toggle() {
        open = !open;
        HelpTextLeft.enabled = open;
        HelpTextRight.enabled = open;
    }
}