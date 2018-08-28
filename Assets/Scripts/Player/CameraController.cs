using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Controls the first- and third-person cameras.
 * Adapted from http://wiki.unity3d.com/index.php?title=SmoothFollowWithCameraBumper
 */

public class CameraController : MonoBehaviour {

    private const float distanceFromHitWall = .125f;//.5f;
    private const float wallDistanceCheck = 5;
    private static readonly Vector3 distancefromPlayer = new Vector3(0, 0, -wallDistanceCheck);

    public static Camera ActiveCamera { get; private set; }

    private static Camera thirdPersonCamera;
    private static Camera firstPersonCamera;
    private static Transform player;
    private static Transform lookAtTarget;

    private static bool firstPerson = false;
    public static bool FirstPerson {
        get {
            return firstPerson;
        }
        set {
            firstPerson = value;
            if (value) {
                thirdPersonCamera.gameObject.SetActive(false);
                firstPersonCamera.gameObject.SetActive(true);
                firstPersonCamera.cullingMask = thirdPersonCamera.cullingMask;
                ActiveCamera = firstPersonCamera;
                Clear();
            } else {
                firstPersonCamera.gameObject.SetActive(false);
                thirdPersonCamera.gameObject.SetActive(true);
                thirdPersonCamera.cullingMask = firstPersonCamera.cullingMask;
                ActiveCamera = thirdPersonCamera;
                Clear();
            }
        }
    }
    private static bool clampCamera = true;
    public static bool ClampCamera {
        get {
            return clampCamera;
        }
        set {
            clampCamera = value;
            if(value)
                ClampY();
        }
    }
    public static float SensitivityX { get; set; } = 2.5f;
    public static float SensitivityY { get; set; } = 2.5f;

    public static bool CameraIsLocked { get; private set; }

    private static float currentX = 0;
    private static float currentY = 0;
    
    void Awake() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>().Find("Body");
        lookAtTarget = player.Find("CameraLookAtTarget").GetComponent<Transform>();
        thirdPersonCamera = lookAtTarget.Find("ThirdPersonCamera").GetComponent<Camera>();
        firstPersonCamera = player.Find("FirstPersonCamera").GetComponent<Camera>();
        //walledCameraHeight = firstPersonCamera.transform.localPosition.y;
        FirstPerson = false;
        Clear();
        UnlockCamera();
    }

    private void LateUpdate() {
        if (CameraIsLocked) {
            if (GamepadController.UsingGamepad) {
                currentX += Input.GetAxis("HorizontalRight") * SensitivityX;
                currentY -= Input.GetAxis("VerticalRight") * SensitivityY;
            } else {
                currentX += Input.GetAxis("Mouse X") * SensitivityX;
                currentY -= Input.GetAxis("Mouse Y") * SensitivityY;
            }
            if (ClampCamera)
                ClampY();

            RefreshCamera();
        }
    }

    public static void Clear() {
        currentY = player.parent.localEulerAngles.x + 30; // Tilted downward a little
        currentX = player.parent.localEulerAngles.y;
        RefreshCamera();
    }

    private static void RefreshCamera() {
        // Horizontal rotation (rotates player body left and right)
        Quaternion horizontalRotation = Quaternion.Euler(0, currentX, 0);
        player.parent.localRotation = horizontalRotation;
        // Vertical rotation (rotates camera up and down body)
        Quaternion verticalRotation = Quaternion.Euler(currentY, 0, 0);
        ActiveCamera.transform.localRotation = verticalRotation;

        if(!firstPerson) {
            Vector3 wantedPosition = verticalRotation * distancefromPlayer; // local
            RaycastHit hit;
            if (Physics.Raycast(lookAtTarget.position, horizontalRotation * wantedPosition, out hit, wallDistanceCheck, GameManager.IgnorePlayerLayer)) {
                ActiveCamera.transform.position = hit.point + distanceFromHitWall * hit.normal;
            } else {
                ActiveCamera.transform.localPosition = wantedPosition;
            }
        }
    }

    private static void ClampY() {
        currentY = Mathf.Clamp(currentY, -89.99f, 89.99f); // (controls top, controls bottom)
    }

    public static void LockCamera() {
        Cursor.lockState = CursorLockMode.Locked;
        CameraIsLocked = true;
        Cursor.visible = false;
    }

    public static void UnlockCamera() {
        Cursor.lockState = CursorLockMode.None;
        CameraIsLocked = false;
        Cursor.visible = true;
    }

}
