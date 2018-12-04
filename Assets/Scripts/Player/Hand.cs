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
    public CoinPouch Pouch { get; private set; }

    // Use this for initialization
    void Awake() {
        centerOfMass = transform.parent;
        Pouch = GetComponent<CoinPouch>();
        distanceToHand = transform.localPosition.magnitude;
    }

    // If the player is holding down Jump, throw the coin downwards biased against the player's movement.
    // If the player is not jumping, the hand follows the camera.
    void LateUpdate() {
        if (Keybinds.Jump()) {
            float vertical = -Keybinds.Vertical() * baseSteepAngle;
            float horizontal = -Keybinds.Horizontal() * baseSteepAngle;

            if (SettingsMenu.settingsData.controlScheme != SettingsData.Gamepad) {
                // If using keyboard, throw coins at a steeper angle
                vertical *= keyboardSteepAngle;
                horizontal *= keyboardSteepAngle;
            }
            Quaternion newRotation = new Quaternion();
            newRotation.SetLookRotation(new Vector3(horizontal, (-1 + Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1).magnitude), vertical), Vector3.up);
            centerOfMass.rotation = CameraController.CameraDirection * newRotation;
        } else {
            // Rotate hand to look towards reticle target
            RaycastHit hit;
            if (Physics.Raycast(CameraController.ActiveCamera.transform.position, CameraController.ActiveCamera.transform.forward, out hit, 1000, GameManager.Layer_IgnorePlayer)) {
                centerOfMass.LookAt(hit.point);
            } else {
                centerOfMass.eulerAngles = CameraController.ActiveCamera.transform.eulerAngles;
            }
        }
    }

    public Coin WithdrawCoinToHand() {
        LateUpdate();
        if (Pouch.Count > 0) {
            Coin coin;
            // Raycast towards the hand. If the raycast hits something, spawn the coin there to prevent it from going through walls.
            RaycastHit hit;
            if (Physics.Raycast(centerOfMass.position, transform.position - centerOfMass.position, out hit, distanceToHand, GameManager.Layer_IgnorePlayer)) {
                coin = Pouch.RemoveCoin(hit.point + hit.normal * coinSize);
            } else {
                coin = Pouch.RemoveCoin(transform.position);
            }
            return coin;
        }
        return null;
    }

    //public Coin SpawnCoin(Vector3 position) {
    //    if (Pouch.Count > 0) {
    //        Coin coin = Pouch.RemoveCoin(position);
    //        return coin;
    //    }
    //    return null;
    //}

    public void CatchCoin(Coin coin) {
        Pouch.AddCoin(coin);
    }

    public void Clear() {
        Pouch.Clear();
    }

}
