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
    public static Camera ActiveCamera { get; private set; }

    private static Camera thirdPersonCamera;
    private static Camera firstPersonCamera;
    private static Transform playerBody;
    private static Transform playerLookAtTarget;
    private static Transform externalPositionTarget; // Assigned by another part of the program for tracking
    private static Transform externalLookAtTarget; // Assigned by another part of the program for tracking

    private static float currentX = 0;
    private static float currentY = 0;
    private static bool cameraIsLocked;


    // Returns the horizontal direction the camera is facing (only in the x/z plane)
    public static Quaternion CameraDirection {
        get {
            Vector3 eulers = ActiveCamera.transform.eulerAngles;
            eulers.x = 0;
            eulers.z = 0;
            return Quaternion.Euler(eulers);
        }
    }


    void Awake() {
        playerBody = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        playerLookAtTarget = transform.GetChild(0);
        thirdPersonCamera = playerLookAtTarget.GetChild(0).GetComponent<Camera>();
        firstPersonCamera = playerLookAtTarget.GetChild(1).GetChild(0).GetComponent<Camera>();
        ActiveCamera = thirdPersonCamera;
        Clear();
        UnlockCamera();
    }

    private void LateUpdate() {
        transform.position = playerBody.transform.position;
        playerLookAtTarget.rotation = Quaternion.Euler(0, currentX, 0);

        if (cameraIsLocked) {
            if (externalPositionTarget) {
                // Called when the Camera is being controlled by some other source, i.e. HarmonyTarget
                ActiveCamera.transform.position = Vector3.Lerp(ActiveCamera.transform.position, externalPositionTarget.position, lerpConstant * Time.deltaTime);
                ActiveCamera.transform.LookAt(externalLookAtTarget);
            } else {
                float deltaX;
                float deltaY;
                if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) {
                    deltaX = Input.GetAxis("HorizontalRight") * SettingsMenu.settingsData.gamepadSensitivityX;
                    deltaY = Input.GetAxis("VerticalRight") * SettingsMenu.settingsData.gamepadSensitivityY;
                } else {
                    deltaX = Input.GetAxis("Mouse X") * SettingsMenu.settingsData.mouseSensitivityX;
                    deltaY = Input.GetAxis("Mouse Y") * SettingsMenu.settingsData.mouseSensitivityY;
                }
                // deltaY is normally the negative of the above statements, so an uninverted camera should be negatative
                if (SettingsMenu.settingsData.cameraInvertX == 1)
                    deltaX = -deltaX;
                if (SettingsMenu.settingsData.cameraInvertY == 0)
                    deltaY = -deltaY;
                currentX += deltaX;
                currentY += deltaY;
                if (SettingsMenu.settingsData.cameraClamping == 1)
                    ClampY();
                UpdateCamera();
            }
        }
    }

    private static void UpdateCamera() {
        if (!externalPositionTarget) {
            // Horizontal rotation (rotates playerBody body left and right)
            // Vertical rotation (rotates camera up and down body)
            Quaternion verticalRotation = Quaternion.Euler(currentY, 0, 0);
            ActiveCamera.transform.localRotation = verticalRotation;

            if (SettingsMenu.settingsData.cameraFirstPerson == 0) {
                Vector3 wantedPosition = verticalRotation * distancefromPlayer; // local
                ActiveCamera.transform.localPosition = wantedPosition;
                if (Physics.Raycast(playerLookAtTarget.position, ActiveCamera.transform.position - playerLookAtTarget.position, out RaycastHit hit, wallDistanceCheck, GameManager.Layer_IgnoreCamera)) {
                    ActiveCamera.transform.position = hit.point + distanceFromHitWall * hit.normal;
                } else {
                    // Check if the camera is just barely touching a wall (check 6 directions)
                    if (Physics.Raycast(ActiveCamera.transform.position, Vector3.down, out hit, distanceFromHitWall, GameManager.Layer_IgnoreCamera)) {
                        ActiveCamera.transform.position = hit.point + distanceFromHitWall * hit.normal;
                    } else if (Physics.Raycast(ActiveCamera.transform.position, Vector3.up, out hit, distanceFromHitWall, GameManager.Layer_IgnoreCamera)) {
                        ActiveCamera.transform.position = hit.point + distanceFromHitWall * hit.normal;
                    } else if (Physics.Raycast(ActiveCamera.transform.position, Vector3.left, out hit, distanceFromHitWall, GameManager.Layer_IgnoreCamera)) {
                        ActiveCamera.transform.position = hit.point + distanceFromHitWall * hit.normal;
                    } else if (Physics.Raycast(ActiveCamera.transform.position, Vector3.right, out hit, distanceFromHitWall, GameManager.Layer_IgnoreCamera)) {
                        ActiveCamera.transform.position = hit.point + distanceFromHitWall * hit.normal;
                    } else if (Physics.Raycast(ActiveCamera.transform.position, Vector3.forward, out hit, distanceFromHitWall, GameManager.Layer_IgnoreCamera)) {
                        ActiveCamera.transform.position = hit.point + distanceFromHitWall * hit.normal;
                    } else if (Physics.Raycast(ActiveCamera.transform.position, Vector3.back, out hit, distanceFromHitWall, GameManager.Layer_IgnoreCamera)) {
                        ActiveCamera.transform.position = hit.point + distanceFromHitWall * hit.normal;
                    }
                }
            }
        }
    }

    public static void Clear() {
        externalPositionTarget = null;
        externalLookAtTarget = null;
        firstPersonCamera.transform.localPosition = Vector3.zero;
        Vector3 eulers = Player.PlayerInstance.transform.eulerAngles;
        //eulers.x = 0;
        //eulers.z = 0;
        playerLookAtTarget.rotation = Quaternion.Euler(eulers);
        currentY = playerLookAtTarget.localEulerAngles.x; // Tilted downward a little
        if (currentY >= 180)
            currentY -= 360;
        currentX = playerLookAtTarget.localEulerAngles.y;
        if (Player.PlayerIronSteel.IsBurningIronSteel) // Update blue lines when the camera is reset
            Player.PlayerIronSteel.SearchForMetals();
        UpdateCamera();
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

    public static void SetExternalSource(Transform position, Transform lookAt) {
        externalPositionTarget = position;
        externalLookAtTarget = lookAt;
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
