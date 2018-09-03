using UnityEngine;
using System.IO;
/*
 * Stores the variables determining each setting that are read from and written to the JSON configuration file.
 */
[System.Serializable]
public class SettingsData : ScriptableObject {

    private const string configFileName = "config.json";
    
    public int controlScheme;
    public int rumble;
    public int pushControlStyle;
    public float allomanticConstant;

    public SettingsData() {
        LoadSettings();
    }

    public void LoadSettings() {
        StreamReader reader = new StreamReader(configFileName, true);
        string jSONText = reader.ReadToEnd();
        reader.Close();

        JsonUtility.FromJsonOverwrite(jSONText, this);
    }

    public void SaveSettings() {


    }

    /*
     * For the JsonUtility to work, all setting-holding variables must be stored in this class.
     * SetData assigns data to the variable with the name name.
     * 
     * Unity is annoying and doesn't support C#7 features like ref functions,
     * so we're having to get the variables by their string names every time.
     * 
     * really liking that O(n) efficiency
     */
    public bool SetData(string name, int data) {
        switch (name) {
            case "controlScheme": {
                    controlScheme = data;
                    return true;
                }
            case "rumble": {
                    rumble = data;
                    return true;
                }
            case "pushControlStyle": {
                    pushControlStyle = data;
                    return true;
                }
            default: {
                    Debug.LogError("SetData with invalid ID: " + name);
                    return false;
                }
        }
    }
    public bool SetData(string name, float data) {
        switch (name) {
            case "allomanticConstant": {
                    allomanticConstant = data;
                    return true;
                }
            default: {
                    Debug.LogError("SetData with invalid ID: " + name);
                    return false;
                }
        }
    }

    public int GetDataInt(string name) {
        switch (name) {
            case "controlScheme": {
                    return controlScheme;
                }
            case "rumble": {
                    return rumble;
                }
            case "pushControlStyle": {
                    return pushControlStyle;
                }
            default: {
                    Debug.LogError("GetDataInt with invalid ID: " + name);
                    return -1;
                }
        }
    }

    public float GetDataFloat(string name) {
        switch (name) {
            case "allomanticConstant": {
                    return allomanticConstant;
                }
            default: {
                    Debug.LogError("GetDataFloat with invalid ID: " + name);
                    return -1;
                }
        }
    }

}
