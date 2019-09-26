using UnityEngine;
using System.Collections;

/*
 * A piece of tech that the Sphere can interface with, opening a console for communication.
 */
public class Interfaceable : Source {

    private bool Locked { get; set; }

    private void Awake() {
        Locked = false;
    }

    // When player contacts the small tigger at the base of the bowl, they "enter it"
    // and are locked into the bowl.
    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("PlayerBody")) {
            // set player position to the bowl
            other.transform.position = transform.position;
            // remove player control
            Locked = true;
            Player.CanControl = false;
        }
    }

    private void Update() {
        if(Locked) {
            // if player jumps, exit
            if (Keybinds.Jump()) {
                FreePlayer();
            } else {
                // Rebind controls: Push goes Up, Pull goes Down
            }
        }
    }

    private void FreePlayer() {
        Locked = false;
        Player.CanControl = true;
    
    }
}
