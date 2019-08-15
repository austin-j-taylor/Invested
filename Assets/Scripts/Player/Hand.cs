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
    private const float spreadSize = .005f;
    public const int spraySize = 3;
    private float distanceToHand;
    public readonly Vector3 coinThrowSpeed = new Vector3(0, 0, 5);

    private Transform centerOfMass;
    private AllomanticIronSteel allomancer;
    public CoinPouch Pouch { get; private set; }

    // Use this for initialization
    void Awake() {
        centerOfMass = transform.parent;
        Pouch = GetComponent<CoinPouch>();
        allomancer = GetComponentInParent<AllomanticIronSteel>();
        distanceToHand = transform.localPosition.magnitude;
    }

    private void RotateHand() {
    }

    public Coin WithdrawCoinToHand() {
        if (Pouch.Count > 0) {
            Coin coin;
            RaycastHit hit;

            // If the player is holding down Jump, throw the coin downwards biased against the player's movement.
            // If the player is not jumping, the hand follows the camera.
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
                if (Physics.Raycast(CameraController.ActiveCamera.transform.position, CameraController.ActiveCamera.transform.forward, out hit, 1000, GameManager.Layer_IgnorePlayer)) {
                    centerOfMass.LookAt(hit.point);
                } else {
                    centerOfMass.eulerAngles = CameraController.ActiveCamera.transform.eulerAngles;
                }
            }

            // Raycast towards the hand. If the raycast hits something, spawn the coin there to prevent it from going through walls.
            if (Physics.Raycast(centerOfMass.position, transform.position - centerOfMass.position, out hit, distanceToHand, GameManager.Layer_IgnoreCamera)) {
                coin = Pouch.RemoveCoin(hit.point + hit.normal * coinSize);
            } else {
                coin = Pouch.RemoveCoin(transform.position);
            }
            // If the wielder of this pouch is not simultaneously Pushing on the coin, add their velocity to the coin
            // The intent is that the coin would realisticially always start with the allomancer's velocity,
            //      but that throws off the aim of the coin.
            if (!allomancer.SteelPushing) {
                coin.GetComponent<Rigidbody>().velocity = allomancer.GetComponent<Rigidbody>().velocity + transform.rotation * coinThrowSpeed;
            } else {
                coin.GetComponent<Rigidbody>().velocity = transform.rotation * coinThrowSpeed;
            }
            return coin;
        }
        return null;
    }

    public Coin[] WithdrawCoinSprayToHand() {
        if (Pouch.Count > 0) {
            Coin[] coins = new Coin[spraySize];
            RaycastHit hit;

            // If the player is holding down Jump, throw the coin downwards biased against the player's movement.
            // If the player is not jumping, the hand follows the camera.
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
                if (Physics.Raycast(CameraController.ActiveCamera.transform.position, CameraController.ActiveCamera.transform.forward, out hit, 1000, GameManager.Layer_IgnorePlayer)) {
                    centerOfMass.LookAt(hit.point);
                } else {
                    centerOfMass.eulerAngles = CameraController.ActiveCamera.transform.eulerAngles;
                }
            }

            Vector3 spawnPosition;
            // Raycast towards the hand. If the raycast hits something, spawn the coins there to prevent them from going through walls.
            if (Physics.Raycast(centerOfMass.position, transform.position - centerOfMass.position, out hit, distanceToHand, GameManager.Layer_IgnoreCamera)) {
                spawnPosition = hit.point + hit.normal * coinSize;
            } else {
                spawnPosition = transform.position;
            }

            for(int i = 0; i < spraySize && Pouch.Count > 0; i++) {
                coins[i] = Pouch.RemoveCoin(spawnPosition + Random.insideUnitSphere * spreadSize);
                // If the wielder of this pouch is not simultaneously Pushing on the coins, add their velocity to the coins
                // The intent is that the coins would realisticially always start with the allomancer's velocity,
                //      but that throws off the aim of the coins.
                if (!allomancer.SteelPushing) {
                    coins[i].GetComponent<Rigidbody>().velocity = allomancer.GetComponent<Rigidbody>().velocity + transform.rotation * coinThrowSpeed;
                } else {
                    coins[i].GetComponent<Rigidbody>().velocity = transform.rotation * coinThrowSpeed;
                }
            }

            return coins;
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
}
