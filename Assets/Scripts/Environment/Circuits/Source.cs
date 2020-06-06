using UnityEngine;
using System.Collections;

/*
 * An Investiture power source.
 * Is considered connected to any siblings.
 * Should always be a child of a Node object.
 */
public class Source : Destructable {

    protected Node parent;

    protected override void Awake() {
        base.Awake();
        if(transform.parent)
            parent = transform.parent.GetComponent<Node>();
    }

    public override bool On {
        set {
            base.On = value;
            if(parent != null)
                parent.Refresh();
        }
    }
}
