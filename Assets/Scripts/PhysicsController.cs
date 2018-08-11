
/*
 * Holds enumerations that decide how Pushes and Pulls are calculated
 */ 

public enum ForceDistanceRelationship { InverseSquareLaw, Linear, Exponential }
public enum AnchorBoostMode { AllomanticNormalForce, ExponentialWithVelocity, None }
public enum NormalForceMinimum { Zero, ZeroAndNegate, Disabled }
public enum NormalForceMaximum { AllomanticForce, Disabled }
public enum ExponentialWithVelocitySignage { AllVelocityDecreasesForce, OnlyBackwardsDecreasesForce, BackwardsDecreasesAndForwardsIncreasesForce }
public enum ExponentialWithVelocityRelativity { Relative, Absolute }

public static class PhysicsController {
    public static ForceDistanceRelationship distanceRelationshipMode = ForceDistanceRelationship.Exponential;
    public static AnchorBoostMode anchorBoostMode = AnchorBoostMode.AllomanticNormalForce;
    public static NormalForceMinimum normalForceMinimum = NormalForceMinimum.Zero;
    public static NormalForceMaximum normalForceMaximum = NormalForceMaximum.AllomanticForce;
    public static bool normalForceEquality = false;
    public static ExponentialWithVelocitySignage exponentialWithVelocitySignage = ExponentialWithVelocitySignage.AllVelocityDecreasesForce;
    public static ExponentialWithVelocityRelativity exponentialWithVelocityRelativity = ExponentialWithVelocityRelativity.Relative;
    public static float distanceConstant = 16f;
    public static float velocityConstant = 16f;
    public static bool airResistanceEnabled = true;
    public static bool gravityEnabled = true;

    private const float airDrag = .2f;
    private const float groundedDrag = 3f;

    public static float AirDrag {
        get {
            return (airResistanceEnabled ? airDrag : 0);
        }
    }
    public static float GroundedDrag {
        get {
            return (airResistanceEnabled ? groundedDrag : 0);
        }
    }
}
