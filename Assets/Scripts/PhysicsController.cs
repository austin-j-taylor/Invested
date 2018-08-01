public enum ForceDistanceRelationship { InverseSquareLaw, Linear, Exponential }
public enum AnchorBoostMode { AllomanticNormalForce, ExponentialWithVelocity, None }
public enum ForceDisplayUnits { Newtons, Gs }

public static class PhysicsController {
    public static ForceDistanceRelationship distanceRelationshipMode = ForceDistanceRelationship.Exponential;
    public static ForceDisplayUnits displayUnits = ForceDisplayUnits.Newtons;
    public static AnchorBoostMode anchorBoostMode = AnchorBoostMode.AllomanticNormalForce;
    public static float distanceConstant = 16f;
    public static float velocityConstant = 16f;
}
