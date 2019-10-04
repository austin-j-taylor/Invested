using UnityEngine;
using System.Collections;

/*
 * A piece of tech that the Sphere can interface with, opening a console for communication.
 */
public abstract class Interfaceable : Powered {

    protected readonly Vector2 cameraDistance = new Vector2(5, 5);

    public bool ReceivedReply { get; set; } = false;
    // True while the player is connected and interfacing with this object
    private bool interfaced = false;
    public bool Interfaced {
        get {
            return interfaced;
        }
        protected set {
            if(interfaced != value) {
                interfaced = value;
                if(value) {
                    StartInterfacing();
                } else {
                    StopInterfacing();
                }
            }
        }
    }

    private void FixedUpdate() {
        if (interfaced) {
            FixedUpdateInterfacing();
        }
    }
    private void Update() {
        if (interfaced) {
            UpdateInterfacing();
        }
    }

    // When player contacts the small tigger at the base of the bowl, they "enter it"
    // and are locked into the bowl.
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger && !interfaced) {
            //open the console and do whatever needs to be done in it
            //HUD.ConsoleController.Open();
            Player.CanControl = false;
            StartCoroutine(Interaction());
        }
    }

    protected abstract void StartInterfacing();
    protected abstract void StopInterfacing();
    protected abstract void FixedUpdateInterfacing();
    protected abstract void UpdateInterfacing();

    // Begins when the player starts a connection.
    // Is killed when the player ends the connection.
    protected abstract IEnumerator Interaction();

}
