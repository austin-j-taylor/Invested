using UnityEngine;
using System.IO;
/*
 * Stores the variables determining each setting that are read from and written to the JSON configuration file.
 * Should not be modified outside of a Setting.
 * They are public because the JSON reader needs them to be. If I were in charge I would never let these kids be public.
 */
[System.Serializable]
public class SettingsData : MonoBehaviour {

    private readonly string configFileName = Path.Combine(Application.streamingAssetsPath, "Data" + Path.DirectorySeparatorChar + "config.json");
    private readonly string defaultConfigFileName = Path.Combine(Application.streamingAssetsPath, "Data" + Path.DirectorySeparatorChar + "default_config.json");

    // Constants for setting states
    public const int MK54 = 0; // Mouse/Keyboard with Select and SelectAlternate on 5 and 4, etc.
    public const int MK45 = 1;
    public const int MKEQ = 2;
    public const int MKQE = 3;
    public const int Gamepad = 4;

    public bool UsingGamepad => controlScheme == Gamepad;

    // Gameplay
    public int controlScheme;
    public int gamepadRumble; // 0 for Disabled, 1 for Enabled
    public int cameraFirstPerson; // 0 for third person, 1 for first person
    public float cameraDistance;
    public int cameraClamping; // 0 for Unclamped, 1 for Clamped
    public int cameraInvertX; // 0 for disabled, 1 for inverted
    public int cameraInvertY;
    public float mouseSensitivityX;
    public float mouseSensitivityY;
    public float gamepadSensitivityX;
    public float gamepadSensitivityY;
    // Interface
    public int highlightedTargetOutline; // 0 for Disabled, 1 for Enabled
    public int pullTargetLineColor; // 0 for blue, 1 for light blue, 2 for green
    public int pushTargetLineColor; // 0 for blue, 1 for red
    public int forceUnits; // 0 for G's, 1 for Newtons
    public int forceComplexity; // 0 for net only, 1 for full sums
    public int hudEnabled; // 0 for Disabled, 1 for Enabled
    public int hudForces; // 0 for Disabled, 1 for Enabled
    public int hudMasses; // 0 for Disabled, 1 for Enabled
    public int fpsCounter; // 0 for Disabled, 1 for Enabled
    public int helpOverlay; // 0 for Disabled, 1 for Simple, 2 for Verbose
    // Graphics
    public int renderblueLines; // 0 for Disabled, 1 for Enabled
    public int antialiasing;
    public int ambientOcclusion;
    public int motionBlur;
    public int bloom;
    public int aberration;
    public int vignetteZinc;
    // Audio
    public float audioMaster;
    public float audioMusic;
    public float audioEffects;
    public float audioVoiceBeeps;
    // Allomancy
    public int pushControlStyle; // 0 for percentage, 1 for magnitude
    public int anchoredBoost; // 0 for Disabled, 1 for ANF, 2 for EWF, 3 for DP
    public int normalForceMin; // 0 for Disabled, 1 for zero, 2 for zero and negate
    public int normalForceMax; // 0 for Disabled, 1 for AF
    public int normalForceEquality; // 0 for Unequal, 1 for Equal
    public int exponentialWithVelocitySignage; // 0 for Both Directions Decrease Force, 1 for Moving Towards Decreases, 2 for Moving Away Decreases force, 3 for Symmetrical
    public float velocityConstant;
    public int forceDistanceRelationship; // 0 for Linear, 1 for Inverse Square, 2 for Exponential with Distance
    public float distanceConstant;
    public float allomanticConstant;
    public float maxPushRange;
    public float metalDetectionThreshold;
    // World
    public int playerGravity; // 0 for Disabled, 1 for Enabled, 2 for Inverted
    public int playerAirResistance; // 0 for Disabled, 1 for Enabled
    public float timeScale;

    private void Awake() {
        LoadSettings();
    }

    public void LoadSettings() {
        try {
            StreamReader reader = new StreamReader(configFileName, true);

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

            StreamWriter writer = new StreamWriter(configFileName, false);
            writer.Write(jSONText);
            writer.Close();

            // Manually apply certain setting effects
            GameManager.AudioManager.SetAudioLevels(audioMaster, audioMusic, audioEffects, audioVoiceBeeps);
            TimeController.CurrentTimeScale = timeScale;
            GraphicsController.SetAntialiasing(antialiasing == 1);
            GraphicsController.SetAmbientOcclusion(ambientOcclusion == 1);
            GraphicsController.SetMotionBlur(motionBlur == 1);
            GraphicsController.SetBloom(bloom == 1);

        } catch (DirectoryNotFoundException e) {
            Debug.LogError(e.Message);
        }
    }

    public void ResetToDefaults() {
        try {
            StreamReader reader = new StreamReader(defaultConfigFileName, true);
            reader.ReadLine(); // get rid of comments in first line
            reader.ReadLine();
            string jSONText = reader.ReadToEnd();
            reader.Close();

            JsonUtility.FromJsonOverwrite(jSONText, this);

            SaveSettings();

        } catch (DirectoryNotFoundException e) {
            Debug.LogError(e.Message);
        }
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
            case "cameraFirstPerson": {
                    cameraFirstPerson = data;
                    return true;
                }
            case "cameraClamping": {
                    cameraClamping = data;
                    return true;
                }
            case "cameraInvertX": {
                    cameraInvertX = data;
                    return true;
                }
            case "cameraInvertY": {
                    cameraInvertY = data;
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
            case "hudForces": {
                    hudForces = data;
                    return true;
                }
            case "hudEnabled": {
                    hudEnabled = data;
                    return true;
                }
            case "hudMasses": {
                    hudMasses = data;
                    return true;
                }
            case "fpsCounter": {
                    fpsCounter = data;
                    return true;
                }
            case "helpOverlay": {
                    helpOverlay = data;
                    return true;
                }
            case "renderblueLines": {
                    renderblueLines = data;
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
            case "highlightedTargetOutline": {
                    highlightedTargetOutline = data;
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
            case "aberration": {
                    aberration = data;
                    return true;
                }
            case "vignetteZinc": {
                    vignetteZinc = data;
                    return true;
                }
            case "pushControlStyle": {
                    pushControlStyle = data;
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
            case "cameraDistance": {
                    cameraDistance = data;
                    return true;
                }
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
            case "audioMaster": {
                    audioMaster = data;
                    return true;
                }
            case "audioMusic": {
                    audioMusic= data;
                    return true;
                }
            case "audioEffects": {
                    audioEffects = data;
                    return true;
                }
            case "audioVoiceBeeps": {
                    audioVoiceBeeps = data;
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
            case "timeScale": {
                    timeScale = data;
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
            case "cameraFirstPerson": {
                    return cameraFirstPerson;
                }
            case "cameraClamping": {
                    return cameraClamping;
                }
            case "cameraInvertX": {
                    return cameraInvertX;
                }
            case "cameraInvertY": {
                    return cameraInvertY;
                }
            case "forceUnits": {
                    return forceUnits;
                }
            case "forceComplexity": {
                    return forceComplexity;
                }
            case "hudForces": {
                    return hudForces;
                }
            case "hudEnabled": {
                    return hudEnabled;
                }
            case "hudMasses": {
                    return hudMasses;
                }
            case "fpsCounter": {
                    return fpsCounter;
                }
            case "helpOverlay": {
                    return helpOverlay;
                }
            case "renderblueLines": {
                    return renderblueLines;
                }
            case "pullTargetLineColor": {
                    return pullTargetLineColor;
                }
            case "pushTargetLineColor": {
                    return pushTargetLineColor;
                }
            case "highlightedTargetOutline": {
                    return highlightedTargetOutline;
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
            case "aberration": {
                    return aberration;
                }
            case "vignetteZinc": {
                    return vignetteZinc;
                }
            case "pushControlStyle": {
                    return pushControlStyle;
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
            case "cameraDistance": {
                    return cameraDistance;
                }
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
            case "audioMaster": {
                    return audioMaster;
                }
            case "audioMusic": {
                    return audioMusic;
                }
            case "audioEffects": {
                    return audioEffects;
                }
            case "audioVoiceBeeps": {
                    return audioVoiceBeeps;
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
            case "timeScale": {
                    return timeScale;
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
