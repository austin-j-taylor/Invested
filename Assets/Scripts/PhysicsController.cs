public enum ForceCalculationMode { InverseSquareLaw, Linear, Exponential }
public enum ForceDisplayUnits { Newtons, Gs }

public static class PhysicsController {
    public static ForceCalculationMode calculationMode = ForceCalculationMode.Linear;
    public static ForceDisplayUnits displayUnits = ForceDisplayUnits.Newtons;
    public static float exponentialConstantC = 8f;
}
