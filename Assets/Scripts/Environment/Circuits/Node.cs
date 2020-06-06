using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Represents a connected system of elements. The immediate children of this object are what are connected.
// If any element in the system is on, the node is On.
// If all elements in the system are off, the node is off.
public class Node : Source {

    public Source[] sources;
    private Powered[] receivers;
    [SerializeField]
    private Node[] connectedNodes = null; // Breaking heirarchy, these nodes are also connected to this one.

    // An element is in this node if it is an immediate child of this game object.
    protected override void Awake() {
        base.Awake();
        
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
        // Make sure connected nodes know we're connected to them
        if (connectedNodes != null) {
            for (int n = 0; n < connectedNodes.Length; n++) {
                connectedNodes[n].ConnectNode(this);
            }
        }

        sources = sourcesList.ToArray();
        receivers = receiversList.ToArray();
    }

    private void Start() {
        Refresh();
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
        // Also check state of other connected nodes.
        if(connectedNodes != null) {
            for (int n = 0; n < connectedNodes.Length; n++) {
                for(int i = 0; i < connectedNodes[n].sources.Length; i++) {
                    if(connectedNodes[n].sources[i].On) {
                        state = true;
                        break;
                    }
                }
            }
        }
        // If any are on, it is on. If all are off, it is off.
        // Set new state of child elements
        for (int i = 0; i < receivers.Length; i++) {
            receivers[i].On = state;
        }
        // Also set state of other connected nodes.
        if (connectedNodes != null) {
            for (int n = 0; n < connectedNodes.Length; n++) {
                for (int i = 0; i < connectedNodes[n].receivers.Length; i++) {
                    connectedNodes[n].receivers[i].On = state;
                }
                connectedNodes[n].On = state;
            }
        }
        On = state;
    }

    // Dynamically connects newNode to this node, regardless of positions in the heirarchy.
    private void ConnectNode(Node newNode) {
        Node[] newArray = new Node[connectedNodes.Length + 1];
        for (int n = 0; n < connectedNodes.Length; n++)
            newArray[n] = connectedNodes[n];
        newArray[newArray.Length - 1] = newNode;
        connectedNodes = newArray;
    }
}
