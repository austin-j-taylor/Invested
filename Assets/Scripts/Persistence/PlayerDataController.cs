using UnityEngine;
using System.IO;

// Handles saving certain data associated with the player, like time trial completions.
[System.Serializable]
public class PlayerDataController : MonoBehaviour {

    private readonly string fileName = Path.Combine(Application.streamingAssetsPath, "Data" + Path.DirectorySeparatorChar + "playerData.json");

    // Data (must be public for JSON)
    [HideInInspector]
    public double timeTrial_TestingGrounds, reachGoal_TestingGrounds, timeTrial_Luthadel;
    
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
            switch (name) {
                case "timeTrial_TestingGrounds": 
                    instance.timeTrial_TestingGrounds = time;
                    break;
                case "reachGoal_TestingGrounds": 
                    instance.reachGoal_TestingGrounds = time;
                    break;
                case "timeTrial_Luthadel": 
                    instance.timeTrial_TestingGrounds = time;
                    break;
                default: 
                        Debug.LogError("SetTimeTrial with invalid ID: " + name);
                break;
        }
        instance.Refresh();
    }

    public static double GetTime(string name) {
        switch(name) {
            case "timeTrial_TestingGrounds":
                return instance.timeTrial_TestingGrounds;
            case "reachGoal_TestingGrounds":
                return instance.reachGoal_TestingGrounds;
            case "timeTrial_Luthadel":
                return instance.timeTrial_Luthadel;
            default:
                Debug.LogError("GetTime with invalid ID: " + name);
                return -1;
        }
    }

}
