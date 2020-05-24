using UnityEngine;
using System.Collections;

/*
 * An Investiture power source.
 */
public class Source : Destructable {

    Node parent;

    private void Awake() {
        parent = transform.parent.GetComponent<Node>();
    }

    public override bool On {
        set {
            base.On = value;
            if(parent)
                parent.Refresh();
        }
    }
}
