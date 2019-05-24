using UnityEngine;
using System.IO;

// Triggers flags when certain conditions are met
[System.Serializable]
public class FlagsController : MonoBehaviour {

    private const string flagsFileName = "Data/flags.json";

    // Flags
    public bool controlSchemeChosen;
    public bool helpOverlayFull;

    private static FlagsController instance;

    public static bool ControlSchemeChosen {
        get {
            return instance.controlSchemeChosen;
        }
        set {
            instance.controlSchemeChosen = value;
            instance.SaveSettings();
            HUD.HelpOverlayController.UpdateText();
        }
    }

    public static bool HelpOverlayFull {
        get {
            return instance.helpOverlayFull;
        }
        set {
            instance.helpOverlayFull = value;
            instance.SaveSettings();
            HUD.HelpOverlayController.UpdateText();
        }
    }

    private void Awake() {
        instance = this;
        LoadSettings();
    }

    public void LoadSettings() {
        try {
            StreamReader reader = new StreamReader(flagsFileName, true);
            string jSONText = reader.ReadToEnd();
            reader.Close();

            JsonUtility.FromJsonOverwrite(jSONText, this);

        } catch (DirectoryNotFoundException e) {
            Debug.LogError(e.Message);
        }
    }

    public void SaveSettings() {
        try {
            string jSONText = JsonUtility.ToJson(this, true);

            StreamWriter writer = new StreamWriter(flagsFileName, false);
            writer.Write(jSONText);
            writer.Close();

        } catch (DirectoryNotFoundException e) {
            Debug.LogError(e.Message);
        }
    }
}
