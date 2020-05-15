using UnityEngine;
using System.Collections;

/*
 * An Investiture power source.
 */
public class Source : Destructable {

    // All circuit elements that are connected to this node.
    [SerializeField]
    private Powered[] connected = null;

    /*
     * Called when this power source turns on or off.
     * Go through all connected elements. If there are no connected power sources that are still on,
     *  turn all the elements off.
     */
    protected void PowerConnected(bool on) {
        if(on) { // turn everything on, no questions asked
            foreach (Powered powered in connected) {
                if(powered != null && !powered.GetComponent<Source>() && powered.gameObject != gameObject)
                    powered.On = on;
            }
        } else {
            bool nodeIsOn = false;
            foreach (Powered powered in connected) {
                if (powered != null && powered.GetComponent<Source>() && powered.gameObject != gameObject && powered.On) { // if there is an active source on this node, don't remove power
                    nodeIsOn = true;
                    break;
                }
            }
            foreach (Powered powered in connected) {
                if (powered != null && !powered.GetComponent<Source>() && powered.gameObject != gameObject)
                    powered.On = nodeIsOn;
            }
        }


    }
}
