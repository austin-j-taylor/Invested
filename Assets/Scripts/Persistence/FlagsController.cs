using UnityEngine;
using System.IO;

// Triggers flags when certain conditions are met
[System.Serializable]
public class FlagsController : MonoBehaviour {

    private readonly string flagsFileName = Application.streamingAssetsPath + "/Data/flags.json";

    // Flags
    // can't make them private because json-parsing has no brains
    public bool controlSchemeChosen;
    public bool helpOverlayFull;
    public bool helpOverlayFuller;

    // Use these externally just to feel better about yourself
    public static bool ControlSchemeChosen {
        get {
            return instance.controlSchemeChosen;
        }
        set {
            if(instance.controlSchemeChosen != value) {
                instance.controlSchemeChosen = value;
                instance.Refresh();
            } else {
                instance.controlSchemeChosen = value;
            }
        }
    }
    public static bool HelpOverlayFull {
        get {
            return instance.helpOverlayFull;
        }
        set {
            if (instance.helpOverlayFull != value) {
                instance.helpOverlayFull = value;
                instance.Refresh();
            } else {
                instance.helpOverlayFull = value;
            }
        }
    }
    public static bool HelpOverlayFuller {
        get {
            return instance.helpOverlayFuller;
        }
        set {
            if (instance.helpOverlayFuller != value) {
                instance.helpOverlayFuller = value;
                instance.Refresh();
            } else {
                instance.helpOverlayFuller = value;
            }
        }
    }

    private static FlagsController instance;

    private void Awake() {
        instance = this;
        LoadSettings();
    }

    private void Refresh() {
        SaveSettings();
        HUD.HelpOverlayController.UpdateText();
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
