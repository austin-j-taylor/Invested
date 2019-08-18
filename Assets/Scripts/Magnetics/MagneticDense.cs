using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A magnetic with a set density.
 */
public class MagneticDense : Magnetic
{
    [SerializeField]
    private float density = 0;

    protected new void Awake() {

        if (netMass == 0) {
            Rb = GetComponentInParent<Rigidbody>();

            if (!Rb) {
                Rb = gameObject.AddComponent<Rigidbody>();
                Rb.SetDensity(density);
                netMass = Rb.mass;
                
                DestroyImmediate(Rb);
                IsStatic = true;
            } else {
                Rb.SetDensity(density);
            }
        } else {
            GetComponentInParent<Rigidbody>().SetDensity(density);
        }

        base.Awake();
    }
}
