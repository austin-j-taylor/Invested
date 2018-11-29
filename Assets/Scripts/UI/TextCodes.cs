using UnityEngine;
using System.Collections;
/*
 * Contains several static fields referenced in on-screen text.
 * For example, if a text field says "You can Pull metals", the word "Pull" should be in blue.
 * Typing "You can " + TextCodes.Pull + " metals.
 * 
 * Also contains strings that will return a certain controller button depending on the control scheme
 *      i.e. will return "Mouse Button 4" or "Q" or "Left Bumper" depending on the control scheme
 */
public class TextCodes : MonoBehaviour {
    UnityEngine.UI.Text tester;
    private void Update() {
        tester = GameObject.FindGameObjectWithTag("Testing").GetComponent<UnityEngine.UI.Text>();
        if(tester) tester.text = "If you want to " + Pull + ", " + KeyPull + ".";
    }
    // Methods that accept strings as arguments and return strings colored in their respective color
    public static string Blue(string s) {
        return "<color=#0080ff>" + s + "</color>";
    }
    public static string MidBlue(string s) {
        return "<color=#00bfff>" + s + "</color>";
    }
    public static string Cyan(string s) {
        return "<color=#00ffff>" + s + "</color>";
    }
    public static string LightRed(string s) {
        return "<color=#ff4040>" + s + "</color>";
    }
    public static string Red(string s) {
        return "<color=#ff0000>" + s + "</color>";
    }

    // Known words that should always appear in a specific color
    public static string Pull {
        get {
            return MidBlue("Pull");
        }
    }
    public static string Push {
        get {
            return Red("Push");
        }
    }

    // Returns the (colored) button input that corresponds to the command, depending on the control scheme
    public static string KeyPull {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return "Press " + MidBlue("Right Trigger");
            else
                return MidBlue("Left-click");
        }
    }
    public static string KeyPush {
        get {
            return Red(TextKeyPush);
        }
    }
    public static string KeySelect {
        get {
            return Cyan(TextKeySelect);
        }
    }
    public static string KeySelectAlternate {
        get {
            return Red(TextKeySelectAlternate);
        }
    }

    // Private methods for return plaintext white-colored button inputs
    private static string TextKeyPull {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return "Right Trigger";
            else
                return "Left Click";
        }
    }
    private static string TextKeyPush {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return "Left Trigger";
            else
                return "Right Click";
        }
    }
    private static string TextKeySelect {
        get {
            switch(SettingsMenu.settingsData.controlScheme) {
                case SettingsData.MK54: {
                        return "Mouse button 5";
                    }
                case SettingsData.MK45: {
                        return "Mouse button 4";
                    }
                case SettingsData.MKEQ: {
                        return "E";
                    }
                case SettingsData.MKQE: {
                        return "Q";
                    }
                default: {
                        return "Right Bumper";
                    }
            }
        }
    }
    private static string TextKeySelectAlternate {
        get {
            switch(SettingsMenu.settingsData.controlScheme) {
                case SettingsData.MK54: {
                        return "Mouse button 4";
                    }
                case SettingsData.MK45: {
                        return "Mouse button 5";
                    }
                case SettingsData.MKEQ: {
                        return "Q";
                    }
                case SettingsData.MKQE: {
                        return "E";
                    }
                default: {
                        return "Left Bumper";
                    }
            }
        }
    }


}
