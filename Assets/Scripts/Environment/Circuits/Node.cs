using UnityEngine;
using System.Collections;

// Represents a connected system of elements.
// If any element in the system is on, the node is HIGH.
// If all elements in the system are off, the node is LOW.
public class Node : MonoBehaviour {

    Source[] sources;
    Powered[] receivers;

    private enum State { Low, High }

    void Awake() {
        sources = GetComponentsInChildren<Source>();
        Powered[] elements = GetComponentsInChildren<Powered>();
        receivers = new Powered[elements.Length - sources.Length];
        int i, j = 0;
        for(i = 0; i < elements.Length; i++) {
            if(elements[i].GetComponent<Source>()) {
                // skip all elements that are sources
                continue;
            } else {
                receivers[j] = elements[i];
                j++;
            }
        }
    }

    /*
     * Called when an element in this node changes state.
     * Go through all connected elements. If there are no connected power sources that are still on,
     *  turn all the elements off.
     */
    public void Refresh() {
        // Learn new state of node
        State newState = State.Low;
        for(int i = 0; i < sources.Length; i++) {
            if(sources[i].On) {
                newState = State.High;
                break;
            }
        }
        // Set new state of child elements
        for(int i = 0; i < receivers.Length; i++) {
            receivers[i].On = (newState == State.High) ? true : false;
        }
    }
}
