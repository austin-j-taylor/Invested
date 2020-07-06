using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Reflection;

/// <summary>
/// Handles setting and getting flags for when certain conditions are met.
/// Includes completed levels and unlocked abilities.
/// These are all flags that can become true, but never return to false once unlocked.
/// (unless through Data Management)
/// </summary>
[System.Serializable]
public class FlagsController : MonoBehaviour {

    #region constants
    private readonly string flagsFileName = Path.Combine(Application.streamingAssetsPath, "Data" + Path.DirectorySeparatorChar + "flags.json");
    #endregion

    #region flagFields
    // Flags
    // can't make them private because json-parsing needs them to be public
    public bool completeTutorial1, completeTutorial2, completeTutorial3, completeTutorial4, completeMARL1, completeMARL2, completeMARL3, completeMARL4,
                completeTestingGrounds, completeShootingGrounds, completeSouthernMountains, completeSeaOfMetal, completeIlluminatingStorms, completeLuthadel;
    public bool controlSchemeChosen;
    public bool pwr_steel, pwr_pewter, pwr_zinc, pwr_coins; // for unlocking abilities
    public bool wheel_area, wheel_bubble;
    // Be wary of adding any other public booleans, as DeleteAllData will reset them.
    #endregion

    public static FlagsController instance;

    private void Awake() {
        instance = this;
        LoadSettings();
    }

    private void Refresh() {
        SaveJSON();
        HUD.UpdateText();
    }

    #region JSON
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
    #endregion

    #region flagAccessors
    public static void SetFlag(string name) {

        FieldInfo field = instance.GetType().GetField(name);
        if (field == null) {
            Debug.LogError("SetFlag with invalid name: " + name);
        }
        field.SetValue(instance, true);

        //Debug.Log("Set flag: " + name);
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
    /// <summary>
    /// Gets the flag field with the given name.
    /// Alternatively, just use FlagsController.instance.theName
    /// </summary>
    /// <param name="name">the name of the field</param>
    /// <returns></returns>
    public static bool GetData(string name) {
        FieldInfo field = instance.GetType().GetField(name);
        if (field == null) {
            Debug.LogError("GetData with invalid name: " + name);
        }
        return (bool)field.GetValue(instance);
    }

    // Clear all flags and abilities, except for the ControlSchemeSelected.
    public static void DeleteAllData() {
        foreach (FieldInfo field in instance.GetType().GetFields()) {
            if(field.FieldType == typeof(bool))
                field.SetValue(instance, false);
        }
        instance.SaveJSON();
        HUD.ControlWheelController.RefreshLocked();
    }
    #endregion
}
