using UnityEngine;
using System.IO;
/*
 * Stores the variables determining each setting that are read from and written to the JSON configuration file.
 * Should not be modified outside of a Setting.
 * They are public because the JSON reader needs them to be. If I were in charge I would never let these kids be public.
 */
[System.Serializable]
public class SettingsData : MonoBehaviour {

    private const string configFileName = "config.json";
    
    // Gameplay
    public int controlScheme; // 0 for MK45, 1 for MKQE, 2 for Gamepad
    public int gamepadRumble;
    public int pushControlStyle; // 0 for percentage, 1 for magnitude
    public int cameraFirstPerson; // 0 for third person, 1 for first person
    public int cameraClamping;
    public float mouseSensitivityX;
    public float mouseSensitivityY;
    public float gamepadSensitivityX;
    public float gamepadSensitivityY;
    // Interface
    public int renderblueLines;
    public int forceUnits; // 0 for Gravity, 1 for Newtons
    public int forceComplexity; // 0 for net only, 1 for full sums
    public int hudForces;
    public int hudMasses;
    // Allomancy
    public int anchoredBoost; // 0 for ANF, 1 for EWF, 2 for Disabled
    public int normalForceMin; // 0 for zero, 1 for zero and negate, 2 for Disabled
    public int normalForceMax; // 0 for ANF, 1 for Disabled
    public int normalForceEquality; // 0 for Unequal, 1 for Equal
    public int exponentialWithVelocitySignage; // 0 for All Decreases Force, 1 for Only Backwards Decreases force, 2 for Backwards Decreases & Forwards Increases
    public int exponentialWithVelocityRelativity; // 0 for Relative, 1 for Absolute
    public float velocityConstant;
    public int forceDistanceRelationship; // 0 for Inverse Square, 1 for Linear, 2 for Exponential
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
