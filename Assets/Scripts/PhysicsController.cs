public enum ForceCalculationMode { InverseSquareLaw, Linear }
public enum ForceDisplayUnits { Newtons, Gs }

public static class PhysicsController {
    public static ForceCalculationMode calculationMode = ForceCalculationMode.Linear;
    public static ForceDisplayUnits displayUnits = ForceDisplayUnits.Newtons;
}
