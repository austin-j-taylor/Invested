using UnityEngine;
using System.IO;
using System.Collections;
using System.Reflection;

/// <summary>
/// Base class for one of the headers in the Settings menu, like "Controls", "Interface", or "Allomancy".
/// Handles saving/writing to the JSON config file.
/// </summary>
public abstract class JSONSettings : MonoBehaviour {

    protected virtual string ConfigFileName => "";
    protected virtual string DefaultConfigFileName => "";

    Setting[] settings;

    private void Awake() {
        settings = GetComponentsInChildren<Setting>();
        LoadSettings(false);
    }

    public void RefreshSettings() {
        foreach (Setting setting in settings) {
            setting.RefreshData();
            setting.RefreshText();
        }
    }

    #region JSON
    public void LoadSettings(bool setSettingsChanged = true) {
        try {
            StreamReader reader = new StreamReader(ConfigFileName, true);

            string jSONText = reader.ReadToEnd();
            reader.Close();

            JsonUtility.FromJsonOverwrite(jSONText, this);

            if (setSettingsChanged)
                SetSettingsWhenChanged();

        } catch (DirectoryNotFoundException e) {
            Debug.LogError(e.Message);
        }
    }

    public void SaveSettings() {
        try {
            string jSONText = JsonUtility.ToJson(this, true);

            StreamWriter writer = new StreamWriter(ConfigFileName, false);
            writer.Write(jSONText);
            writer.Close();

            SetSettingsWhenChanged();

        } catch (DirectoryNotFoundException e) {
            Debug.LogError(e.Message);
        }
    }
    /// <summary>
    /// Reset these settings to their default values.
    /// </summary>
    public void ResetToDefaults() {
        try {
            StreamReader reader = new StreamReader(DefaultConfigFileName, true);
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
    #endregion

    #region settingAccessors
    /// <summary>
    /// Sets the setting with the given name.
    /// </summary>
    /// <param name="name">the name of the field for the setting</param>
    /// <param name="data">the new value for that setting</param>
    /// <returns></returns>
    public bool SetData(string name, int data) {
        FieldInfo field = GetType().GetField(name);
        if (field == null) {
            Debug.LogError("SetData with invalid name: " + name);
            return false;
        }
        field.SetValue(this, data);
        return true;
    }
    /// <summary>
    /// Sets the setting with the given name.
    /// </summary>
    /// <param name="name">the name of the field for the setting</param>
    /// <param name="data">the new value for that setting</param>
    /// <returns></returns>
    public bool SetData(string name, float data) {
        FieldInfo field = GetType().GetField(name);
        if (field == null) {
            Debug.LogError("SetData with invalid name: " + name);
            return false;
        }
        field.SetValue(this, data);
        return true;
    }
    /// <summary>
    /// Gets the value for the setting.
    /// </summary>
    /// <param name="name">the name of the setting to get</param>
    /// <returns>the value for that setting</returns>
    public int GetDataInt(string name) {
        FieldInfo field = GetType().GetField(name);
        if (field == null) {
            Debug.LogError("GetDataInt with invalid name: " + name);
            return -1;
        }
        return (int)field.GetValue(this);
    }
    /// <summary>
    /// Gets the value for the setting.
    /// </summary>
    /// <param name="name">the name of the setting to get</param>
    /// <returns>the value for that setting</returns>
    public float GetDataFloat(string name) {
        FieldInfo field = GetType().GetField(name);
        if (field == null) {
            Debug.LogError("GetDataFloat with invalid name: " + name);
            return -1;
        }
        return (float)field.GetValue(this);
    }
    #endregion

    /// <summary>
    /// Manually apply certain setting effects when they are changed
    /// </summary>
    public virtual void SetSettingsWhenChanged() { }
}
