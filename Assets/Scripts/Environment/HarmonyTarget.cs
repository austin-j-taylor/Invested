using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarmonyTarget : MonoBehaviour {

    private const float distanceThreshold = .001f;
    private const float forceConstantFar = 40f;
    private const float forceConstantClose = 10000f;
    private const float dragConstantClose = 10f;

    private bool playerHasEntered;

    private Rigidbody rb;
    private Animator anim;
    private Renderer[] symbolRenderers;
    private Transform harmonySphere;
    private Transform inner;
    private Transform cameraPositionTarget;
    private Transform cameraLookAtTarget;

    private void Awake() {
        rb = GetComponentInChildren<Rigidbody>();
        anim = GetComponent<Animator>();
        harmonySphere = rb.GetComponent<Transform>();
        cameraPositionTarget = transform.GetChild(0);
        inner = transform.GetChild(1);
        cameraLookAtTarget = inner.GetChild(0);

        symbolRenderers = GetComponentsInChildren<Renderer>();
        
        harmonySphere.gameObject.AddComponent<HarmonySphere>();

        playerHasEntered = false;
    }

    private void LateUpdate() {
        if (!playerHasEntered) {
            // Rotate Target towards Player
            Vector3 distancetoPlayer = harmonySphere.position - CameraController.ActiveCamera.transform.position;// player.transform.position;
            float angle = 180 + Mathf.Atan2(distancetoPlayer.x, distancetoPlayer.z) * Mathf.Rad2Deg;
            Vector3 newRotation = Vector3.zero;
            //angle = Mathf.LerpAngle(inner.eulerAngles.y, angle, Time.deltaTime * 10f);

            newRotation.y = angle;
            transform.eulerAngles = newRotation;
        }
    }

    private void FixedUpdate() {
        if (!playerHasEntered) {
            // Pull sphere towards center of Harmony Target
            Vector3 distanceToInner = inner.position - harmonySphere.position;
            float sqrDistance = distanceToInner.sqrMagnitude;
            if (sqrDistance > distanceThreshold) {
                rb.AddForce(forceConstantFar * distanceToInner.normalized * sqrDistance, ForceMode.Acceleration);
            }
        } else {
            // Pull player towards center of Harmony Target
            Vector3 distance = Player.PlayerIronSteel.CenterOfMass - inner.position;
            Player.PlayerIronSteel.GetComponent<Rigidbody>().AddForce(-forceConstantClose * distance, ForceMode.Force);
            Vector3 pos = cameraPositionTarget.position;
            pos.y = cameraLookAtTarget.position.y;
            cameraPositionTarget.position = pos;
            //rb.AddForce(forceConstantClose * distance, ForceMode.Force);

            // So the light from the sphere doesn't flicker around the player's body
            harmonySphere.position = Player.PlayerIronSteel.CenterOfMass;
        }
    }

    private void BeginAnimation() {
        playerHasEntered = true;
        Player.CanControlPlayer = false;
        harmonySphere.GetComponent<Renderer>().enabled = false;
        harmonySphere.GetComponent<Collider>().enabled = false;
        anim.SetTrigger("PlayerHasEntered");
        HUD.DisableHUD();
        
        Player.PlayerIronSteel.StopBurningIronSteel();
        Player.PlayerInstance.GetComponent<Rigidbody>().useGravity = false;
        Player.PlayerInstance.GetComponent<Rigidbody>().drag = dragConstantClose;
        CameraController.ExternalPositionTarget = cameraPositionTarget;
        CameraController.ExternalLookAtTarget = cameraLookAtTarget;

    }

    private void EndAnimation() {
        Player.PlayerInstance.GetComponentInChildren<MeshRenderer>().material = harmonySphere.GetComponent<Renderer>().material;
        foreach (Renderer renderer in symbolRenderers)
            renderer.material = GameManager.Material_Ettmetal_Glowing;
        
        // Open menus

    }

    private void EndLevel() {
        playerHasEntered = false;
    }

    private class HarmonySphere : MonoBehaviour {
        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Player") && !other.isTrigger) {
                // Player has touched the target.
                GetComponentInParent<HarmonyTarget>().BeginAnimation();
            }
        }
    }

}
