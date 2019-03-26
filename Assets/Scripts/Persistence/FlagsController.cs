using UnityEngine;
using System.IO;

// Triggers flags when certain conditions are met
[System.Serializable]
public class FlagsController : MonoBehaviour {

    private const string flagsFileName = "Assets/Data/flags.json";

    // Flags
    public bool controlSchemeChosen;

    private static FlagsController instance;

    public static bool ControlSchemeChosen {
        get {
            return instance.controlSchemeChosen;
        }
        set {
            instance.controlSchemeChosen = true;
            instance.SaveSettings();
        }
    }

    private void Awake() {
        instance = this;
        LoadSettings();
    }
    
    public void LoadSettings() {
        StreamReader reader = new StreamReader(flagsFileName, true);
        string jSONText = reader.ReadToEnd();
        reader.Close();

        JsonUtility.FromJsonOverwrite(jSONText, this);
    }

    public void SaveSettings() {
        string jSONText = JsonUtility.ToJson(this, true);

        StreamWriter writer = new StreamWriter(flagsFileName, false);
        writer.Write(jSONText);
        writer.Close();
    }
}
