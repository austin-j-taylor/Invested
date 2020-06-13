using UnityEngine;
using System.IO;
using System.Reflection;

// Handles saving certain data associated with the player, like time trial completions.
[System.Serializable]
public class PlayerDataController : MonoBehaviour {

    private readonly string fileName = Path.Combine(Application.streamingAssetsPath, "Data" + Path.DirectorySeparatorChar + "playerData.json");

    // Data (must be public for JSON)
    [HideInInspector]
    public double timeTrial_TestingGrounds, reachGoal_TestingGrounds, breakTargets_TestingGrounds, timeTrial1_Luthadel, timeTrial2_Luthadel, breakTargets_Luthadel, breakTargets_ShootingGrounds;
    
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
}
