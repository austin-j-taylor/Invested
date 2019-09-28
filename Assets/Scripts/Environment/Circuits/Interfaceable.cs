using UnityEngine;
using System.Collections;

/*
 * A piece of tech that the Sphere can interface with, opening a console for communication.
 */
public abstract class Interfaceable : Powered {

    public bool ReceivedReply { get; set; } = false;

    // When player contacts the small tigger at the base of the bowl, they "enter it"
    // and are locked into the bowl.
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger) {
            //open the console and do whatever needs to be done in it
            HUD.ConsoleController.Open();
            Player.CanControl = false;
            StartCoroutine(Interaction());
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger) {
            HUD.ConsoleController.Close();
            Player.CanControl = true;
            StopAllCoroutines();
        }
    }
    
    // Begins when the player starts a connection.
    // Is killed when the player ends the connection.
    protected abstract IEnumerator Interaction();

}
