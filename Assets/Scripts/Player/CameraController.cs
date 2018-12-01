using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Controls the first- and third-person cameras.
 * Adapted from http://wiki.unity3d.com/index.php?title=SmoothFollowWithCameraBumper
 */

public class CameraController : MonoBehaviour {

    private const float distanceFromHitWall = .125f;//.5f;
    private const float wallDistanceCheck = 4;
    private const float lerpConstant = 5;
    private static readonly Vector3 distancefromPlayer = new Vector3(0, 0, -wallDistanceCheck);
    private static readonly Vector3 firstPersonCameraHeight = new Vector3(0, 1, 0);

    public static Camera ActiveCamera { get; private set; }
    public static Transform ExternalPositionTarget { get; set; } // Assigned by another part of the program for tracking
    public static Transform ExternalLookAtTarget { get; set; } // Assigned by another part of the program for tracking

    private static Camera thirdPersonCamera;
    private static Camera firstPersonCamera;
    private static Transform playerBody;
    private static Transform playerLookAtTarget;

    private static float currentX = 0;
    private static float currentY = 0;
    private static bool cameraIsLocked;

    void Awake() {
        playerBody = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>().Find("Body");
        playerLookAtTarget = playerBody.Find("CameraLookAtTarget").GetComponent<Transform>();
        thirdPersonCamera = playerLookAtTarget.Find("ThirdPersonCamera").GetComponent<Camera>();
        //thirdPersonCamera.gameObject.AddComponent<CameraCollisionResolver>();
        firstPersonCamera = playerBody.Find("FirstPersonCamera").GetComponent<Camera>();
        ActiveCamera = thirdPersonCamera;
        Clear();
        UnlockCamera();
    }

    private void LateUpdate() {
        if (cameraIsLocked) {
            if (Player.CanControlPlayer) {
                if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) {
                    currentX += Input.GetAxis("HorizontalRight") * SettingsMenu.settingsData.gamepadSensitivityX;
                    currentY -= Input.GetAxis("VerticalRight") * SettingsMenu.settingsData.gamepadSensitivityY;
                } else {
                    currentX += Input.GetAxis("Mouse X") * SettingsMenu.settingsData.mouseSensitivityX;
                    currentY -= Input.GetAxis("Mouse Y") * SettingsMenu.settingsData.mouseSensitivityY;
                }
                if (SettingsMenu.settingsData.cameraClamping == 1)
                    ClampY();
            }
            UpdateCamera();
        }
        if (ExternalPositionTarget) {
            UpdateCameraToExternalSource();
        }
    }

    public static void Clear() {
        ExternalPositionTarget = null;
        ExternalLookAtTarget = null;
        firstPersonCamera.transform.localPosition = firstPersonCameraHeight;
        currentY = playerBody.parent.localEulerAngles.x + 30; // Tilted downward a little
        currentX = playerBody.parent.localEulerAngles.y;
        UpdateCamera();
    }

    private static void UpdateCamera() {
        if (Player.CanControlPlayer) {
            // Horizontal rotation (rotates playerBody body left and right)
            Quaternion horizontalRotation = Quaternion.Euler(0, currentX, 0);
            playerBody.parent.localRotation = horizontalRotation;
            // Vertical rotation (rotates camera up and down body)
            Quaternion verticalRotation = Quaternion.Euler(currentY, 0, 0);
            ActiveCamera.transform.localRotation = verticalRotation;

            if (SettingsMenu.settingsData.cameraFirstPerson == 0) {
                Vector3 wantedPosition = verticalRotation * distancefromPlayer; // local
                ActiveCamera.transform.localPosition = wantedPosition;
                RaycastHit hit;
                if (Physics.Raycast(playerLookAtTarget.position, horizontalRotation * wantedPosition, out hit, wallDistanceCheck, GameManager.Layer_IgnorePlayer)) {
                    ActiveCamera.transform.position = hit.point + distanceFromHitWall * hit.normal;
                } else {
                    // Check if the camera is just barely touching a wall (check 6 directions)
                    if (Physics.Raycast(ActiveCamera.transform.position, Vector3.down, out hit, distanceFromHitWall)) {
                        ActiveCamera.transform.position = hit.point + distanceFromHitWall * hit.normal;
                    } else if (Physics.Raycast(ActiveCamera.transform.position, Vector3.up, out hit, distanceFromHitWall)) {
                        ActiveCamera.transform.position = hit.point + distanceFromHitWall * hit.normal;
                    } else if (Physics.Raycast(ActiveCamera.transform.position, Vector3.left, out hit, distanceFromHitWall)) {
                        ActiveCamera.transform.position = hit.point + distanceFromHitWall * hit.normal;
                    } else if (Physics.Raycast(ActiveCamera.transform.position, Vector3.right, out hit, distanceFromHitWall)) {
                        ActiveCamera.transform.position = hit.point + distanceFromHitWall * hit.normal;
                    } else if (Physics.Raycast(ActiveCamera.transform.position, Vector3.forward, out hit, distanceFromHitWall)) {
                        ActiveCamera.transform.position = hit.point + distanceFromHitWall * hit.normal;
                    } else if (Physics.Raycast(ActiveCamera.transform.position, Vector3.back, out hit, distanceFromHitWall)) {
                        ActiveCamera.transform.position = hit.point + distanceFromHitWall * hit.normal;
                    }


                }
            }
        }
    }
    /*
     * Called when the Camera is being controlled by some other source, i.e. HarmonyTarget
     */
    private void UpdateCameraToExternalSource() {
        ActiveCamera.transform.position = Vector3.Lerp(ActiveCamera.transform.position, ExternalPositionTarget.position, lerpConstant * Time.deltaTime);
        ActiveCamera.transform.LookAt(ExternalLookAtTarget);
    }

    public static void LockCamera() {
        Cursor.lockState = CursorLockMode.Locked;
        cameraIsLocked = true;
        Cursor.visible = false;
    }

    public static void UnlockCamera() {
        Cursor.lockState = CursorLockMode.None;
        cameraIsLocked = false;
        Cursor.visible = true;
    }

    private void ClampY() {
        currentY = Mathf.Clamp(currentY, -89.99f, 89.99f); // (controls top, controls bottom)
    }

    public void SetThirdPerson() {
        firstPersonCamera.gameObject.SetActive(false);
        thirdPersonCamera.gameObject.SetActive(true);
        thirdPersonCamera.cullingMask = firstPersonCamera.cullingMask;
        ActiveCamera = thirdPersonCamera;
        Clear();
    }

    public void SetFirstPerson() {
        thirdPersonCamera.gameObject.SetActive(false);
        firstPersonCamera.gameObject.SetActive(true);
        firstPersonCamera.cullingMask = thirdPersonCamera.cullingMask;
        ActiveCamera = firstPersonCamera;
        Clear();
    }
}
