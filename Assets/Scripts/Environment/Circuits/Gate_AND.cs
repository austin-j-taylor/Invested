using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// When all the Nodes in its immediate children are HIGH, the gate output is HIGH and appears as "ON" to its parent node.
public class Gate_AND : Node {

    public override void Refresh() {
        Debug.Log("refreshed a AND");
        // Learn new state of node
        bool state = true;
        for (int i = 0; i < sources.Length; i++) {
            Debug.Log("chcekd a source: " + sources[i].On);
            if (!sources[i].On) {
                state = false;
                break;
            }
        }
        // Set new state of gate output
        Debug.Log("node is now " + state);
        GetComponent<Source>().On = state;
    }
}
