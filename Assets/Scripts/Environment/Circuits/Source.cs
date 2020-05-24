using UnityEngine;
using System.Collections;

/*
 * An Investiture power source.
 */
public class Source : Destructable {

    // All circuit elements that are connected to this node.
    [SerializeField]
    private Powered[] connected = null;

    public override bool On {
        set {
            base.On = value;
            GetComponentInParent<Node>().Refresh();
        }
    }
}
