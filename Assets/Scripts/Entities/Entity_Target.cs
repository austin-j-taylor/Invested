using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity_Target : Entity {

    protected override void Start() {
        base.Start();

        MaxHealth = 10;
        GetComponent<Source>().On = false;
        GetComponent<Magnetic>().CenterOfMass = transform.InverseTransformPoint(transform.Find("Body").position);
    }

    protected override void Die() {
        base.Die();

        transform.Find("Body").GetComponent<MeshRenderer>().material = GameManager.Material_MARLmetal_lit;
        GetComponent<Magnetic>().enabled = false;
        GetComponent<Source>().On = true;
    }
}
