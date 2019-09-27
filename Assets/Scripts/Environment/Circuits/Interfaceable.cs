using UnityEngine;
using System.Collections;

/*
 * A piece of tech that the Sphere can interface with, opening a console for communication.
 */
public class Interfaceable : Powered {
    
    // When player contacts the small tigger at the base of the bowl, they "enter it"
    // and are locked into the bowl.
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger) {
            Debug.Log("", gameObject);
            Debug.Log("", other.gameObject);
            // set player position to the bowl
            //Player.PlayerInstance.transform.position = transform.position;
            //open the console and do whatever needs to be done in it
            HUD.ConsoleController.Open();
        }
    }


    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger) {
            HUD.ConsoleController.Close();
        }
    }
}
