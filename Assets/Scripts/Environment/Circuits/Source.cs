using UnityEngine;
using System.Collections;

/*
 * An Investiture power source.
 */
public class Source : Destructable {

    protected Node parent;

    private void Awake() {
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
