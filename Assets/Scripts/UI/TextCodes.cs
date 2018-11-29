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

    // Methods that accept strings as arguments and return strings colored in their respective color
    public static string LightBlue(string s) {
        return "<color=#0080ff>" + s + "</color>";
    }
    public static string MidBlue(string s) {
        return "<color=#00bfff>" + s + "</color>";
    }
    public static string Cyan(string s) {
        return "<color=#00ffff>" + s + "</color>";
    }
    public static string Red(string s) {
        return "<color=#ff0000>" + s + "</color>";
    }

    // Known words that should always appear in a specific color
    public static string Pull {
        get {
            return LightBlue("Pull");
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
            return Cyan(TextKeyPull);
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
            if (SettingsMenu.settingsData.controlScheme == 2)
                return "Left Click";
            else
                return "Right Trigger";
        }
    }
    private static string TextKeyPush {
        get {
            if (SettingsMenu.settingsData.controlScheme == 2)
                return "Right Click";
            else
                return "Left Trigger";
        }
    }
    private static string TextKeySelect {
        get {
            switch(SettingsMenu.settingsData.controlScheme) {
                case 0: {
                        return "Mouse button 5";
                    }
                case 1: {
                        return "E";
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
                case 0: {
                        return "Mouse button 4";
                    }
                case 1: {
                        return "Q";
                    }
                default: {
                        return "Left Bumper";
                    }
            }
        }
    }


}
