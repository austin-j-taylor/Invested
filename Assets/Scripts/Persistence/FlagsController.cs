using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Reflection;

// Triggers flags when certain conditions are met
// Includes completed levels and unlocked abilities
// These are all flags that can become true, but never return to false once unlocked.
[System.Serializable]
public class FlagsController : MonoBehaviour {

    private readonly string flagsFileName = Path.Combine(Application.streamingAssetsPath, "Data" + Path.DirectorySeparatorChar + "flags.json");

    // Flags
    // can't make them private because json-parsing needs them to be public
    public bool completeTutorial1, completeTutorial2, completeTutorial3, completeTutorial4, completeMARL1, completeMARL2, completeMARL3, completeMARL4;
    public bool controlSchemeChosen;
    public bool pwr_steel, pwr_pewter, pwr_zinc, pwr_coins; // for unlocking abilities
    public bool wheel_area, wheel_bubble;

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

    public static void SetFlag(string name) {

        instance.GetType().GetField(name).SetValue(instance, true);

        Debug.Log("Set flag: " + name);
        // When ability flags are set, the corresponding power should be immediately unlocked.
        switch (name) {
            case "pwr_steel":
                Player.PlayerIronSteel.SteelReserve.IsEnabled = true;
                break;
            case "pwr_pewter":
                Player.PlayerPewter.PewterReserve.IsEnabled = true;
                break;
            case "pwr_zinc":
                Player.CanControlZinc = true;
                break;
            case "pwr_coins":
                Player.CanThrowCoins = true;
                HUD.ControlWheelController.RefreshLocked();
                break;
            case "wheel_area":
                Player.CanThrowCoins = true;
                HUD.ControlWheelController.RefreshLocked();
                break;
            case "wheel_bubble":
                Player.CanThrowCoins = true;
                HUD.ControlWheelController.RefreshLocked();
                break;
        }

        instance.Refresh();
    }

    public static bool GetData(string name) {
        return (bool)instance.GetType().GetField(name).GetValue(instance);
    }
}
