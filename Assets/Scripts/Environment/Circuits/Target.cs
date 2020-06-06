using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A kind of target to shoot at.
public class Target : SourceBreakable {

    private const int thresholdForce = 1;

    private Magnetic magnetic;

    private Renderer goldRenderer;
    private MaterialPropertyBlock fillBlock;

    protected override void Awake() {
        magnetic = GetComponent<Magnetic>();
        if(magnetic)
            magnetic.CenterOfMass = transform.InverseTransformPoint(transform.Find("Body").position);

        goldRenderer = transform.Find("Body").GetComponent<Renderer>();
        fillBlock = new MaterialPropertyBlock();

        OnByDefault = false;
        base.Awake();
    }

    protected override void Break() {
        base.Break();
        transform.Find("Body").GetComponent<MeshRenderer>().material = GameManager.Material_MARLmetal_lit;
        if (magnetic)
            magnetic.enabled = false;
        fillBlock.SetFloat("_Fill", 0);
        goldRenderer.SetPropertyBlock(fillBlock);
    }
    protected override void Repair() {
        base.Repair();
        transform.Find("Body").GetComponent<MeshRenderer>().material = GameManager.Material_MARLmetal_unlit;
        if (magnetic)
            magnetic.enabled = true;
    }
    protected override void ContinueRepairing() {
        base.ContinueRepairing();
        fillBlock.SetFloat("_Fill", (float)(health / MaxHealth));
        goldRenderer.SetPropertyBlock(fillBlock);
    }

    protected virtual void OnCollisionEnter(Collision collision) {
        if(collision.impulse.magnitude > thresholdForce) {
            StartDestroying();
        }
    }
}
