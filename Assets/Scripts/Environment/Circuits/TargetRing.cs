using UnityEngine;
using System.Collections;

/*
 * A large ring that, when passed through, loses its glow and turns on in the node.
 */
public class TargetRing : Source {

    protected virtual void OnTriggerEnter(Collider other) {
        if (Player.IsPlayerTrigger(other)) {
            if(!On) {
                On = true;
                GetComponent<Renderer>().material.CopyPropertiesFromMaterial(GameManager.Material_MARLmetal_unlit);
                GetComponent<AudioSource>().Play();
            }
        }
    }
}
