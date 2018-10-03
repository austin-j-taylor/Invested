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
    private const string defaultConfigFileName = "default_config.json";
    
    // Gameplay
    public int controlScheme; // 0 for MK45, 1 for MKQE, 2 for Gamepad
    public int gamepadRumble; // 0 for Disabled, 1 for Enabled
    public int pushControlStyle; // 0 for percentage, 1 for magnitude
    public int cameraFirstPerson; // 0 for third person, 1 for first person
    public int cameraClamping; // 0 for Unclamped, 1 for Clamped
    public float mouseSensitivityX;
    public float mouseSensitivityY;
    public float gamepadSensitivityX;
    public float gamepadSensitivityY;
    // Interface
    public int renderblueLines; // 0 for Disabled, 1 for Enabled
    public int highlightedTargetOutline; // 0 for Disabled, 1 for Enabled
    public int pullTargetLineColor; // 0 for blue, 1 for light blue, 2 for green
    public int pushTargetLineColor; // 0 for blue, 1 for red
    public int forceUnits; // 0 for G's, 1 for Newtons
    public int forceComplexity; // 0 for net only, 1 for full sums
    public int hudForces; // 0 for Disabled, 1 for Enabled
    public int hudMasses; // 0 for Disabled, 1 for Enabled
    public int fpsCounter; // 0 for Disabled, 1 for Enabled
    // Graphics
    public int antialiasing;
    public int ambientOcclusion;
    public int motionBlur;
    public int bloom;
    // Allomancy
    public int anchoredBoost; // 0 for Disabled, 1 for ANF, 2 for EWF
    public int normalForceMin; // 0 for Disabled, 1 for zero, 2 for zero and negate
    public int normalForceMax; // 0 for Disabled, 1 for AF
    public int normalForceEquality; // 0 for Unequal, 1 for Equal
    public int exponentialWithVelocitySignage; // 0 for Both Directions Decrease Force, 1 for Only Backwards Decreases force, 2 for Backwards Decreases & Forwards Increases
    public int exponentialWithVelocityRelativity; // 0 for Relative, 1 for Absolute
    public float velocityConstant;
    public int forceDistanceRelationship; // 0 for Linear, 1 for Inverse Square, 2 for Exponential with Distance
    public float distanceConstant;
    public float allomanticConstant;
    public float maxPushRange;
    public float metalDetectionThreshold;
    // World
    public int playerGravity; // 0 for Disabled, 1 for Enabled
    public int playerAirResistance; // 0 for Disabled, 1 for Enabled

    private void Awake() {
        LoadSettings();
    }

    public void LoadSettings() {
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

    public void ResetToDefaults() {
        StreamReader reader = new StreamReader(defaultConfigFileName, true);
        reader.ReadLine(); // get rid of comments in first line
        reader.ReadLine();
        string jSONText = reader.ReadToEnd();
        reader.Close();

        JsonUtility.FromJsonOverwrite(jSONText, this);

        SaveSettings();
    }

    /*
     * For the JsonUtility to work, all setting-holding variables must be stored in this class.
     * SetData assigns data to the variable with the name name.
     * 
     * Unity is annoying and doesn't support C#7 features like ref functions,
     * Nor pointers (without making me feel bad my calling my code "unsafe")
     * so we're having to get the variables by their string names every time.
     * 
     * really liking that O(2*n) efficiency every time you click a button
     */
    public bool SetData(string name, int data) {
        switch (name) {
            case "controlScheme": {
                    controlScheme = data;
                    return true;
                }
            case "gamepadRumble": {
                    gamepadRumble = data;
                    return true;
                }
            case "pushControlStyle": {
                    pushControlStyle = data;
                    return true;
                }
            case "cameraFirstPerson": {
                    cameraFirstPerson = data;
                    return true;
                }
            case "cameraClamping": {
                    cameraClamping = data;
                    return true;
                }
            case "renderblueLines": {
                    renderblueLines = data;
                    return true;
                }
            case "highlightedTargetOutline": {
                    highlightedTargetOutline = data;
                    return true;
                }
            case "pullTargetLineColor": {
                    pullTargetLineColor = data;
                    return true;
                }
            case "pushTargetLineColor": {
                    pushTargetLineColor = data;
                    return true;
                }
            case "forceUnits": {
                    forceUnits = data;
                    return true;
                }
            case "forceComplexity": {
                    forceComplexity = data;
                    return true;
                }
            case "fpsCounter": {
                    fpsCounter = data;
                    return true;
                }
            case "antialiasing": {
                    antialiasing = data;
                    return true;
                }
            case "ambientOcclusion": {
                    ambientOcclusion = data;
                    return true;
                }
            case "motionBlur": {
                    motionBlur = data;
                    return true;
                }
            case "bloom": {
                    bloom = data;
                    return true;
                }
            case "hudForces": {
                    hudForces = data;
                    return true;
                }
            case "hudMasses": {
                    hudMasses = data;
                    return true;
                }
            case "anchoredBoost": {
                    anchoredBoost = data;
                    return true;
                }
            case "normalForceMin": {
                    normalForceMin = data;
                    return true;
                }
            case "normalForceMax": {
                    normalForceMax = data;
                    return true;
                }
            case "normalForceEquality": {
                    normalForceEquality = data;
                    return true;
                }
            case "exponentialWithVelocitySignage": {
                    exponentialWithVelocitySignage = data;
                    return true;
                }
            case "exponentialWithVelocityRelativity": {
                    exponentialWithVelocityRelativity = data;
                    return true;
                }
            case "forceDistanceRelationship": {
                    forceDistanceRelationship = data;
                    return true;
                }
            case "playerGravity": {
                    playerGravity = data;
                    return true;
                }
            case "playerAirResistance": {
                    playerAirResistance = data;
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
            case "mouseSensitivityX": {
                    mouseSensitivityX = data;
                    return true;
                }
            case "mouseSensitivityY": {
                    mouseSensitivityY = data;
                    return true;
                }
            case "gamepadSensitivityX": {
                    gamepadSensitivityX = data;
                    return true;
                }
            case "gamepadSensitivityY": {
                    gamepadSensitivityY = data;
                    return true;
                }
            case "velocityConstant": {
                    velocityConstant = data;
                    return true;
                }
            case "distanceConstant": {
                    distanceConstant = data;
                    return true;
                }
            case "allomanticConstant": {
                    allomanticConstant = data;
                    return true;
                }
            case "maxPushRange": {
                    maxPushRange = data;
                    return true;
                }
            case "metalDetectionThreshold": {
                    metalDetectionThreshold = data;
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
            case "gamepadRumble": {
                    return gamepadRumble;
                }
            case "pushControlStyle": {
                    return pushControlStyle;
                }
            case "cameraFirstPerson": {
                    return cameraFirstPerson;
                }
            case "cameraClamping": {
                    return cameraClamping;
                }
            case "renderblueLines": {
                    return renderblueLines;
                }
            case "highlightedTargetOutline": {
                    return highlightedTargetOutline;
                }
            case "pullTargetLineColor": {
                    return pullTargetLineColor;
                }
            case "pushTargetLineColor": {
                    return pushTargetLineColor;
                }
            case "forceUnits": {
                    return forceUnits;
                }
            case "forceComplexity": {
                    return forceComplexity;
                }
            case "fpsCounter": {
                    return fpsCounter;
                }
            case "antialiasing": {
                    return antialiasing;
                }
            case "ambientOcclusion": {
                    return ambientOcclusion;
                }
            case "motionBlur": {
                    return motionBlur;
                }
            case "bloom": {
                    return bloom;
                }
            case "hudForces": {
                    return hudForces;
                }
            case "hudMasses": {
                    return hudMasses;
                }
            case "anchoredBoost": {
                    return anchoredBoost;
                }
            case "normalForceMin": {
                    return normalForceMin;
                }
            case "normalForceMax": {
                    return normalForceMax;
                }
            case "normalForceEquality": {
                    return normalForceEquality;
                }
            case "exponentialWithVelocitySignage": {
                    return exponentialWithVelocitySignage;
                }
            case "exponentialWithVelocityRelativity": {
                    return exponentialWithVelocityRelativity;
                }
            case "forceDistanceRelationship": {
                    return forceDistanceRelationship;
                }
            case "playerGravity": {
                    return playerGravity;
                }
            case "playerAirResistance": {
                    return playerAirResistance;
                }
            default: {
                    Debug.LogError("GetDataInt with invalid ID: " + name);
                    return -1;
                }
        }
    }

    public float GetDataFloat(string name) {
        switch (name) {
            case "mouseSensitivityX": {
                    return mouseSensitivityX;
                }
            case "mouseSensitivityY": {
                    return mouseSensitivityY;
                }
            case "gamepadSensitivityX": {
                    return gamepadSensitivityX;
                }
            case "gamepadSensitivityY": {
                    return gamepadSensitivityY;
                }
            case "velocityConstant": {
                    return velocityConstant;
                }
            case "distanceConstant": {
                    return distanceConstant;
                }
            case "allomanticConstant": {
                    return allomanticConstant;
                }
            case "maxPushRange": {
                    return maxPushRange;
                }
            case "metalDetectionThreshold": {
                    return metalDetectionThreshold;
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
