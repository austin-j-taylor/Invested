public enum ForceDistanceRelationship { InverseSquareLaw, Linear, Exponential }
public enum AnchorBoostMode { AllomanticNormalForce, ExponentialWithVelocity, None }
public enum ForceDisplayUnits { Newtons, Gs }
public enum NormalForceMaximum { AllomanticForce, Disabled }
public enum NormalForceMinimum { Zero, Disabled, ZeroButNegate }
public enum ExponentialWithVelocityMinimum { BackwardsDecreasesAndForwardsIncreasesForce, OnlyBackwardsDecreasesForce, AllVelocityDecreasesForce }

public static class PhysicsController {
    public static ForceDistanceRelationship distanceRelationshipMode = ForceDistanceRelationship.Exponential;
    public static ForceDisplayUnits displayUnits = ForceDisplayUnits.Newtons;
    public static AnchorBoostMode anchorBoostMode = AnchorBoostMode.AllomanticNormalForce;
    public static NormalForceMaximum normalForceMaximum = NormalForceMaximum.AllomanticForce;
    public static NormalForceMinimum normalForceMinimum = NormalForceMinimum.Zero;
    public static ExponentialWithVelocityMinimum exponentialWithVelocityMinimum = ExponentialWithVelocityMinimum.BackwardsDecreasesAndForwardsIncreasesForce;
    public static float distanceConstant = 16f;
    public static float velocityConstant = 16f;
}
