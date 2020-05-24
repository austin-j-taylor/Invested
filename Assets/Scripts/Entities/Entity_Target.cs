using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity_Target : Entity {

    private Source source;
    private Magnetic magnetic;

    protected override void Start() {
        base.Start();

        MaxHealth = 10;
        source = GetComponent<Source>();
        source.On = false;
        magnetic = GetComponent<Magnetic>();
        if(magnetic)
            magnetic.CenterOfMass = transform.InverseTransformPoint(transform.Find("Body").position);
    }

    protected override void Die() {
        base.Die();
        Debug.Log("dead");
        GetComponent<AudioSource>().Play();
        transform.Find("Body").GetComponent<MeshRenderer>().material = GameManager.Material_MARLmetal_lit;
        if(magnetic)
            magnetic.enabled = false;
        GetComponent<Source>().On = true;
    }

    public bool On {
        get => source.On;
    }
}
