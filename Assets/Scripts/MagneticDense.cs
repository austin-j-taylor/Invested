using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A magnetic with a set density.
 */
public class MagneticDense : Magnetic
{
    [SerializeField]
    private float density;

    protected new void Awake() {
        GetComponentInParent<Rigidbody>().SetDensity(density);
        base.Awake();
    }
}
