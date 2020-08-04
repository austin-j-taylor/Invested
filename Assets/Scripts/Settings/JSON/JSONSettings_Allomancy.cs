using UnityEngine;
using System.Collections;
using System.IO;

public class JSONSettings_Allomancy : JSONSettings {

    protected override string DefaultConfigFileName => Path.Combine(Application.streamingAssetsPath, "Data", "Config", "config_Allomancy_default.json");

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

    protected override void Awake() {
        ConfigFileName = Path.Combine(Application.persistentDataPath, "Data", "Config", "config_Allomancy.json");
        base.Awake();
    }
}
