public enum ForceCalculationMode { InverseSquareLaw, Linear, Exponential }
public enum NormalForceMode { Disabled, Enabled }
public enum ForceDisplayUnits { Newtons, Gs }

public static class PhysicsController {
    public static ForceCalculationMode calculationMode = ForceCalculationMode.Linear;
    public static ForceDisplayUnits displayUnits = ForceDisplayUnits.Newtons;
    public static NormalForceMode normalForceMode = NormalForceMode.Enabled;
    public static float exponentialConstantC = 16f;
}
