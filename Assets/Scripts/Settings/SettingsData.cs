using UnityEngine;
using System.IO;
using System.Reflection;
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
    public int velocityZoom;
    public int postProcessingEnabled;
    public int antialiasing;
    public int ambientOcclusion;
    public int motionBlur;
    public int bloom;
    public int aberration;
    public int vignetteZinc;
    public int clouds;
    public int fullscreen;
    public float resolution;
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
            //GraphicsController.SetAntialiasing(antialiasing == 1);
            //GraphicsController.SetAmbientOcclusion(ambientOcclusion == 1);
            //GraphicsController.SetMotionBlur(motionBlur == 1);
            //GraphicsController.SetBloom(bloom == 1);
            GraphicsController.SetFullscreenResolution(resolution, (FullScreenMode)fullscreen);

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
        FieldInfo field = GetType().GetField(name);
        if(field == null) {
            Debug.LogError("SetData with invalid name: " + name);
            return false;
        }
        field.SetValue(this, data);
        return true;
    }
    public bool SetData(string name, float data) {
        FieldInfo field = GetType().GetField(name);
        if (field == null) {
            Debug.LogError("SetData with invalid name: " + name);
            return false;
        }
        field.SetValue(this, data);
        return true;
    }

    public int GetDataInt(string name) {
        FieldInfo field = GetType().GetField(name);
        if (field == null) {
            Debug.LogError("GetDataInt with invalid name: " + name);
            return -1;
        }
        return (int)field.GetValue(this);
    }
    public float GetDataFloat(string name) {
        FieldInfo field = GetType().GetField(name);
        if (field == null) {
            Debug.LogError("GetDataFloat with invalid name: " + name);
            return -1;
        }
        return (float)field.GetValue(this);
    }
}
