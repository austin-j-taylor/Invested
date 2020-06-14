using UnityEngine;
using System.IO;
using System.Reflection;

// Handles saving certain data associated with the player, like time trial completions.
[System.Serializable]
public class PlayerDataController : MonoBehaviour {

    private static readonly string fileName = Path.Combine(Application.streamingAssetsPath, "Data" + Path.DirectorySeparatorChar + "playerData.json");
    private static readonly string defaultFileName = Path.Combine(Application.streamingAssetsPath, "Data" + Path.DirectorySeparatorChar + "default_playerData.json");

    // Data (must be public for JSON)
    public double timeTrial_TestingGrounds, reachGoal_TestingGrounds, breakTargets_TestingGrounds, timeTrial1_Luthadel, timeTrial2_Luthadel, breakTargets_Luthadel, breakTargets_ShootingGrounds;

    // Be wary of adding any other fields, as DeleteAllData will reset them.

    private static PlayerDataController instance;

    private void Awake() {
        instance = this;
        LoadData();
    }

    private void Refresh() {
        SaveJSON();
        HUD.UpdateText();
    }

    public void LoadData() {
        try {
            StreamReader reader = new StreamReader(fileName, true);
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

            StreamWriter writer = new StreamWriter(fileName, false);
            writer.Write(jSONText);
            writer.Close();

        } catch (DirectoryNotFoundException e) {
            Debug.LogError(e.Message);
        }
    }

    public static void SetTimeTrial(string name, double time) {
        FieldInfo field = instance.GetType().GetField(name);
        if (field == null) {
            Debug.LogError("SetTimeTrial with invalid name: " + name);
        }
        field.SetValue(instance, time);
        instance.Refresh();
    }
    public static double GetTime(string name) {
        FieldInfo field = instance.GetType().GetField(name);
        if (field == null) {
            Debug.LogError("GetTime with invalid name: " + name);
            return -1;
        }
        return (double)field.GetValue(instance);
    }

    // Set all time records back to their defaults.
    public static void DeleteAllData() {
        try {
            // Read defaults into this object
            StreamReader reader = new StreamReader(defaultFileName, true);
            string jSONText = reader.ReadToEnd();
            reader.Close();

            JsonUtility.FromJsonOverwrite(jSONText, instance);
            // Write all default times back to file
            StreamWriter writer = new StreamWriter(fileName, false);
            writer.Write(jSONText);
            writer.Close();

        } catch (DirectoryNotFoundException e) {
            Debug.LogError(e.Message);
        }
    }
}
