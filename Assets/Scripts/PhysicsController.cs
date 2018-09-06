
/*
 * Holds enumerations that decide how Pushes and Pulls are calculated
 */ 


public static class PhysicsController {
    //public static ForceDistanceRelationship distanceRelationshipMode = ForceDistanceRelationship.Exponential;
    //public static AnchorBoostMode anchorBoostMode = AnchorBoostMode.AllomanticNormalForce;
    //public static NormalForceMinimum normalForceMinimum = NormalForceMinimum.Zero;
    //public static NormalForceMaximum normalForceMaximum = NormalForceMaximum.AllomanticForce;
    //public static bool normalForceEquality = true;
    //public static ExponentialWithVelocitySignage exponentialWithVelocitySignage = ExponentialWithVelocitySignage.AllVelocityDecreasesForce;
    //public static ExponentialWithVelocityRelativity exponentialWithVelocityRelativity = ExponentialWithVelocityRelativity.Relative;
    //public static float distanceConstant = 16f;
    //public static float velocityConstant = 16f;
    //public static bool airResistanceEnabled = true;
    //public static bool gravityEnabled = true;

    private const float airDrag = .2f;
    private const float groundedDrag = 3f;

    public static float AirDrag {
        get {
            // You: "why use ints to represent binary values that should be represented by booleans"
            // Me, an intellectual:
            return (SettingsMenu.settingsData.playerAirResistance * airDrag);
        }
    }
    public static float GroundedDrag {
        get {
            return (SettingsMenu.settingsData.playerAirResistance * groundedDrag);
        }
    }
}
