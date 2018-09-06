using UnityEngine;
using System.IO;
/*
 * Stores the variables determining each setting that are read from and written to the JSON configuration file.
 */
[System.Serializable]
public class SettingsData : MonoBehaviour {

    private const string configFileName = "config.json";
    
    // Gameplay
    public int controlScheme;
    public int gamepadRumble;
    public int pushControlStyle;
    public int cameraPerspective;
    public int cameraClamping;
    public float mouseSensitivityX;
    public float mouseSensitivityY;
    public float gamepadSensitivityX;
    public float gamepadSensitivityY;
    // Interface
    public int renderblueLines;
    public int forceUnits;
    public int forceComplexity;
    public int hudforces;
    public int hudMasses;
    // Allomancy
    public int anchoredBoost;
    public int normalForceMin;
    public int normalForceMax;
    public int normalForceEquality;
    public int exponentialWithVelocitySignage;
    public int exponentialWithVelocityRelativity;
    public float exponentialConstant;
    public int forceDistanceRelationship;
    public float distanceConstant;
    public float allomanticConstant;
    public float maxPushRange;
    // World
    public int playerGravity;
    public int playerAirResistance;



    private void Awake() {
        LoadSettings();
    }

    private void LoadSettings() {
        StreamReader reader = new StreamReader(configFileName, true);
        string jSONText = reader.ReadToEnd();
        reader.Close();

        JsonUtility.FromJsonOverwrite(jSONText, this);
    }

    public void SaveSettings() {
        string jSONText = JsonUtility.ToJson(this, true);

        StreamWriter writer = new StreamWriter(configFileName, false);
        writer.Write(jSONText);
        writer.Close();
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
                    gamepadRumble = data;
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
                    return gamepadRumble;
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

    ///*
    // * Just in case the player quits with the settings menu still open
    // */
    //private void OnApplicationQuit() {
    //    SaveSettings();
    //}
}
