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
        // For repairable targets, decide to not repair if all sibling targets have been shot before this one finished repairing.
        bool shouldStayDead = true;
        bool foundSibling = false;
        for(int i = 0; i < parent.sources.Length; i++) {
            Target sibling = parent.sources[i].GetComponent<Target>();
            if (sibling == null || !sibling.repairWhenDestroyed || sibling == this) {
                // Not a relevant target, ignore.
                continue;
            }
            foundSibling = true;
            if(!sibling.On) {
                shouldStayDead = false;
            }
        }
        if (foundSibling && shouldStayDead) {
            // Die for good
            return;
        }
        
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
        if(collision.relativeVelocity.magnitude > thresholdForce) {
            StartDestroying();
        }
    }
}
