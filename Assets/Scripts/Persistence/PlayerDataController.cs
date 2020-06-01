using UnityEngine;
using System.IO;

// Handles saving certain data associated with the player, like time trial completions and abilities.
[System.Serializable]
public class PlayerDataController : MonoBehaviour {

    private readonly string fileName = Path.Combine(Application.streamingAssetsPath, "Data" + Path.DirectorySeparatorChar + "playerData.json");

    // Data (must be public for JSON)
    [HideInInspector]
    public double timeTrial_TestingGrounds, reachGoal_TestingGrounds, breakTargets_TestingGrounds, timeTrial_Luthadel, breakTargets_Luthadel, reachGoal_Luthadel;
    [HideInInspector]
    public int pwr_controlWheel, pwr_steel, pwr_pewter, pwr_zinc, pwr_coins; // for unlocking abilities
    
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
            case "breakTargets_TestingGrounds":
                instance.breakTargets_TestingGrounds = time;
                break;
            case "timeTrial_Luthadel":
                instance.timeTrial_Luthadel = time;
                break;
            case "reachGoal_Luthadel":
                instance.reachGoal_Luthadel = time;
                break;
            case "breakTargets_Luthadel":
                instance.breakTargets_Luthadel = time;
                break;
            default:
                Debug.LogError("SetTimeTrial with invalid ID: " + name);
                break;
        }
        instance.Refresh();
    }
    public static void SetData(string name, int data) {
        switch (name) {
            case "pwr_steel":
                instance.pwr_steel = data;
                break;
            case "pwr_controlWheel":
                instance.pwr_controlWheel = data;
                break;
            case "pwr_pewter":
                instance.pwr_pewter = data;
                break;
            case "pwr_zinc":
                instance.pwr_zinc = data;
                break;
            case "pwr_coins":
                instance.pwr_coins = data;
                break;
            default:
                Debug.LogError("SetData with invalid ID: " + name);
                break;
        }
        instance.Refresh();
    }

    public static double GetTime(string name) {
        switch (name) {
            case "timeTrial_TestingGrounds":
                return instance.timeTrial_TestingGrounds;
            case "reachGoal_TestingGrounds":
                return instance.reachGoal_TestingGrounds;
            case "breakTargets_TestingGrounds":
                return instance.breakTargets_TestingGrounds;
            case "timeTrial_Luthadel":
                return instance.timeTrial_Luthadel;
            case "reachGoal_Luthadel":
                return instance.reachGoal_Luthadel;
            case "breakTargets_Luthadel":
                return instance.breakTargets_Luthadel;
            default:
                Debug.LogError("GetTime with invalid ID: " + name);
                return -1;
        }
    }

    public static bool UnlockedControlWheel() {
        return instance.pwr_controlWheel == 1;
    }
    public static bool UnlockedSteel() {
        return instance.pwr_steel == 1;
    }
    public static bool UnlockedPewter() {
        return instance.pwr_pewter == 1;
    }
    public static bool UnlockedZinc() {
        return instance.pwr_zinc == 1;
    }
    public static bool UnlockedCoins() {
        return instance.pwr_coins == 1;
    }
}
