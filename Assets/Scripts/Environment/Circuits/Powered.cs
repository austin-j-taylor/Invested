using UnityEngine;
using System.Collections;

/*
 * An element of an Investiture circuit that can be powered on or off.
 */
public class Powered : MonoBehaviour {

    private bool on = false;
    public virtual bool On {
        get {
            return on;
        }
        set {
            on = value;
        }
    }
}
