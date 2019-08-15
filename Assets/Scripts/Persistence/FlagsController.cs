using UnityEngine;
using System.IO;

// Triggers flags when certain conditions are met
[System.Serializable]
public class FlagsController : MonoBehaviour {

    public enum Level { Start01, Complete01, Complete02, Complete03, Complete04 };
    public enum Flag { ControlSchemeChosen, HelpOverlay01, HelpOverlay02, HelpOverlay03 };

    private readonly string flagsFileName = Application.streamingAssetsPath + "/Data/flags.json";

    // Flags
    // can't make them private because json-parsing has no brains
    // "Please don't access these" is the best security I've got
    public bool complete01;
    public bool complete02;
    public bool complete03;
    public bool complete04;
    public bool controlSchemeChosen;
    public bool helpOverlay01;
    public bool helpOverlay02;
    public bool helpOverlay03;
    
    private static FlagsController instance;

    private void Awake() {
        instance = this;
        LoadSettings();
    }

    private void Refresh() {
        SaveJSON();
        HUD.UpdateText();
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

    public void SaveJSON() {
        try {
            string jSONText = JsonUtility.ToJson(this, true);

            StreamWriter writer = new StreamWriter(flagsFileName, false);
            writer.Write(jSONText);
            writer.Close();

        } catch (DirectoryNotFoundException e) {
            Debug.LogError(e.Message);
        }
    }

    public static void SetLevel(Level level) {
        switch(level) {
            case Level.Start01:
                break;
            case Level.Complete01:
                if (!instance.complete01) {
                    instance.complete01 = true;
                }
                break;
            case Level.Complete02:
                if (!instance.complete02) {
                    instance.complete02 = true;
                }
                break;
            case Level.Complete03:
                if (!instance.complete03) {
                    instance.complete03 = true;
                }
                break;
            case Level.Complete04:
                if (!instance.complete04) {
                    instance.complete04 = true;
                }
                break;
        }
        instance.Refresh();
    }

    public static void SetFlag(Flag flag) {
        switch (flag) {
            case Flag.ControlSchemeChosen:
                if(!instance.controlSchemeChosen) {
                    instance.controlSchemeChosen = true;
                }
                break;
            case Flag.HelpOverlay01:
                if (!instance.helpOverlay01) {
                    instance.helpOverlay01 = true;
                }
                break;
            case Flag.HelpOverlay02:
                if (!instance.helpOverlay02) {
                    instance.helpOverlay02 = true;
                }
                break;
            case Flag.HelpOverlay03:
                if (!instance.helpOverlay03) {
                    instance.helpOverlay03 = true;
                }
                break;
        }
        instance.Refresh();
    }
    public static bool GetFlag(Flag flag) {
        switch(flag) {
            case Flag.ControlSchemeChosen:
                return instance.controlSchemeChosen;
            case Flag.HelpOverlay01:
                return instance.helpOverlay01;
            case Flag.HelpOverlay02:
                return instance.helpOverlay02;
            case Flag.HelpOverlay03:
                return instance.helpOverlay03;
        }
        return false; // never reached
    }
    public static bool GetLevel(Level level) {
        switch (level) {
            case Level.Complete01:
                return instance.complete01;
            case Level.Complete02:
                return instance.complete02;
            case Level.Complete03:
                return instance.complete03;
        }
        return false; // never reached
    }
}
