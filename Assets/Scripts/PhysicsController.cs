using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ForceCalculationMode { InverseSquareLaw, Linear }
public enum ForceDisplayUnits { Newtons, Gs }

public class PhysicsController : MonoBehaviour {

    public static ForceCalculationMode calculationMode;
    public static ForceDisplayUnits displayUnits;

    private void Start() {
        calculationMode = ForceCalculationMode.Linear;
        displayUnits = ForceDisplayUnits.Newtons;
    }

}
