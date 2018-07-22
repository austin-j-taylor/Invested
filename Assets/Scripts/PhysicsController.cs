using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ForceCalculationMode { InverseSquareLaw, Linear }

public class PhysicsController : MonoBehaviour {

    public static ForceCalculationMode calculationMode;

    private void Start() {
        calculationMode = ForceCalculationMode.InverseSquareLaw;
    }

}
