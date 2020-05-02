using UnityEngine;
using System.IO;

// Triggers flags when certain conditions are met
[System.Serializable]
public class FlagsController : MonoBehaviour {

    public enum Level { completeTutorial1, completeTutorial2, completeTutorial3, completeTutorial4, completeMARL1, completeMARL2, completeMARL3, completeMARL4 };
    public enum Flag { ControlSchemeChosen };

    private readonly string flagsFileName = Path.Combine(Application.streamingAssetsPath, "Data" + Path.DirectorySeparatorChar + "flags.json");

    // Flags
    // can't make them private because json-parsing has no brains
    // "Please don't access these" is the best security I've got
    public bool completeTutorial1, completeTutorial2, completeTutorial3, completeTutorial4, completeMARL1, completeMARL2, completeMARL3, completeMARL4;
    public bool controlSchemeChosen;
    
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
            case Level.completeTutorial1:
                instance.completeTutorial1 = true;
                break;
            case Level.completeTutorial2:
                instance.completeTutorial2 = true;
                break;
            case Level.completeTutorial3:
                instance.completeTutorial3 = true;
                break;
            case Level.completeTutorial4:
                instance.completeTutorial4 = true;
                break;
            case Level.completeMARL1:
                instance.completeMARL1 = true;
                break;
            case Level.completeMARL2:
                instance.completeMARL2 = true;
                break;
            case Level.completeMARL3:
                instance.completeMARL3 = true;
                break;
            case Level.completeMARL4:
                instance.completeMARL4 = true;
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
        }
        instance.Refresh();
    }
    public static bool GetFlag(Flag flag) {
        switch(flag) {
            case Flag.ControlSchemeChosen:
                return instance.controlSchemeChosen;
        }
        return false; // never reached
    }
    public static bool GetLevel(Level level) {
        switch (level) {
            case Level.completeTutorial1:
                return instance.completeTutorial1;
            case Level.completeTutorial2:
                return instance.completeTutorial2;
            case Level.completeTutorial3:
                return instance.completeTutorial3;
            case Level.completeTutorial4:
                return instance.completeTutorial4;
            case Level.completeMARL1:
                return instance.completeMARL1;
            case Level.completeMARL2:
                return instance.completeMARL2;
            case Level.completeMARL3:
                return instance.completeMARL3;
            case Level.completeMARL4:
                return instance.completeMARL4;
        }
        return false; // never reached
    }
}
