using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Represents a connected system of elements.
// If any element in the system is on, the node is On.
// If all elements in the system are off, the node is off.
public class Node : Powered {

    Node parent;

    public Source[] sources;
    public Powered[] receivers;

    // An element is in this node if it is an immediate child of this game object.
    void Awake() {
        parent = transform.parent.GetComponent<Node>();
        List<Source> sourcesList = new List<Source>();
        List<Powered> receiversList = new List<Powered>();
        // Go through the immediate children
        foreach(Transform child in transform) {
            // If it has a source, add it to sources
            // If it doens't, add it to receivers
            Source source = child.GetComponent<Source>();
            Powered powered = child.GetComponent<Powered>();
            if (source)
                sourcesList.Add(source);
            else if (powered)
                receiversList.Add(powered);
        }
        sources = sourcesList.ToArray();
        receivers = receiversList.ToArray();

        On = false;
    }

    /*
     * Called when an element in this node changes state.
     * Go through all connected elements. If there are no connected power sources that are still on,
     *  turn all the elements off.
     */
    public virtual void Refresh() {
        // Learn new state of node
        bool state = false;
        for(int i = 0; i < sources.Length; i++) {
            if(sources[i].On) {
                state = true;
                break;
            }
        }
        // Set new state of child elements
        for(int i = 0; i < receivers.Length; i++) {
            receivers[i].On = state;
        }
        On = state;
        // Refresh parent
        if (parent)
            parent.Refresh();
    }
}
