public enum ForceDistanceRelationship { InverseSquareLaw, Linear, Exponential }
public enum AnchorBoostMode { AllomanticNormalForce, ExponentialWithVelocity, None }
public enum ForceDisplayUnits { Newtons, Gs }
public enum NormalForceMinimum { Zero, ZeroAndNegate, Disabled }
public enum NormalForceMaximum { AllomanticForce, Disabled }
public enum ExponentialWithVelocitySignage { AllVelocityDecreasesForce, OnlyBackwardsDecreasesForce, BackwardsDecreasesAndForwardsIncreasesForce }
public enum ExponentialWithVelocityRelativity { Relative, Absolute }

public static class PhysicsController {
    public static ForceDistanceRelationship distanceRelationshipMode = ForceDistanceRelationship.Exponential;
    public static ForceDisplayUnits displayUnits = ForceDisplayUnits.Newtons;
    public static AnchorBoostMode anchorBoostMode = AnchorBoostMode.AllomanticNormalForce;
    public static NormalForceMinimum normalForceMinimum = NormalForceMinimum.Zero;
    public static NormalForceMaximum normalForceMaximum = NormalForceMaximum.AllomanticForce;
    public static ExponentialWithVelocitySignage exponentialWithVelocitySignage = ExponentialWithVelocitySignage.AllVelocityDecreasesForce;
    public static ExponentialWithVelocityRelativity exponentialWithVelocityRelativity = ExponentialWithVelocityRelativity.Relative;
    public static float distanceConstant = 16f;
    public static float velocityConstant = 16f;
}
