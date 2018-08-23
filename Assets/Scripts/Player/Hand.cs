using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Controls how and where coins are thrown from the player.
 */
public class Hand : MonoBehaviour {

    private const float baseSteepAngle = 1f / 2f;
    private const float keyboardSteepAngle = 2f / 3f;
    private const float coinSize = .05f;
    private float distanceToHand;

    private Transform centerOfMass;
    private PlayerMovementController movementController;
    private CoinPouch pouch;

    // Use this for initialization
    void Start() {
        centerOfMass = transform.parent;
        movementController = GetComponentInParent<PlayerMovementController>();
        pouch = GetComponent<CoinPouch>();
        distanceToHand = transform.localPosition.magnitude;
    }

    // If the player is in the air, the hand follows the left joystick.
    // If the player is grounded or is in the air but not touching the joystick, the hand follows the camera.
    void Update() {

        if (movementController.IsGrounded) {
            centerOfMass.localEulerAngles = new Vector3(CameraController.ActiveCamera.transform.eulerAngles.x, 0, 0);
        } else {
            float vertical = -Keybinds.Vertical() * baseSteepAngle;
            float horizontal = -Keybinds.Horizontal() * baseSteepAngle;

            if (!GamepadController.UsingGamepad) {
                // If using keyboard, throw coins at a steeper angle
                vertical *= keyboardSteepAngle;
                horizontal *= keyboardSteepAngle;
            }

            Quaternion newRotation = new Quaternion();
            newRotation.SetLookRotation(new Vector3(horizontal, (-1 + Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1).magnitude), vertical), Vector3.up);
            centerOfMass.localRotation = newRotation;
        }
    }

    public Coin WithdrawCoinToHand() {
        if (pouch.Count > 0) {
            Coin coin;
            // Raycast towards the hand. If the raycast hits something, spawn the coin there to prevent it from going through walls.
            RaycastHit hit;
            if (Physics.Raycast(centerOfMass.position, transform.position - centerOfMass.position, out hit, distanceToHand, GameManager.IgnorePlayerLayer)) {
                coin = pouch.RemoveCoin(hit.point + hit.normal * coinSize);
            } else {
                coin = pouch.RemoveCoin(transform.position);
            }
            return coin;
        }
        return null;
    }

    public Coin SpawnCoin(Vector3 position) {
        if (pouch.Count > 0) {
            Coin coin = pouch.RemoveCoin(position);
            return coin;
        }
        return null;
    }

    public void CatchCoin(Coin coin) {
        pouch.AddCoin(coin);
    }

    public void Clear() {
        
    }

}
