using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Controls the first-person camera.
 * Adapted from http://wiki.unity3d.com/index.php?title=SmoothFollowWithCameraBumper
 */

public class CameraController : MonoBehaviour {

    private const float walledCameraHeight = .125f;//.5f;
    private const float wallDistanceCheck = 5;
    private const float distanceFromPlayer = 5f;

    public static Camera ActiveCamera { get; private set; }

    private static Camera thirdPersonCamera;
    private static Camera firstPersonCamera;
    private static Transform player;
    private static Transform targetPosition;

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
    public static bool CameraIsLocked { get; private set; }
    public static float SensitivityX { get; set; } = 2.5f;
    public static float SensitivityY { get; set; } = 2.5f;

    private static float currentX = 0;
    private static float currentY = 0;
    private static Vector3 cameraHeightOffset = new Vector3(0, .5f, 0);
    
    void Awake() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>().Find("Body");
        targetPosition = player.Find("CameraTargetPosition").GetComponent<Transform>();
        firstPersonCamera = player.Find("FirstPersonCamera").GetComponent<Camera>();
        thirdPersonCamera = player.Find("ThirdPersonCamera").GetComponent<Camera>();
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
            currentY = Mathf.Clamp(currentY, -89f, 89.999f);

            RefreshCamera();
        }
    }

    private static void RefreshCamera() {
        Vector3 direction = new Vector3(0, 0, -distanceFromPlayer);
        Quaternion rotation = Quaternion.Euler(currentY, 0, 0);
        player.parent.localRotation = Quaternion.AngleAxis(currentX, player.up);
        targetPosition.localPosition = rotation * direction;

        if (firstPerson) {
            ActiveCamera.transform.localRotation = Quaternion.AngleAxis(currentY, Vector3.right);
        } else {
            Vector3 wantedPosition = targetPosition.position;
            RaycastHit hit;
            if (Physics.Raycast(player.position + cameraHeightOffset, wantedPosition - player.position - cameraHeightOffset, out hit, wallDistanceCheck, GameManager.IgnorePlayerLayer)) {
                Vector3 normal = walledCameraHeight * hit.normal;
                wantedPosition.x = hit.point.x + normal.x;
                wantedPosition.y = hit.point.y + normal.y;
                //wantedPosition.y = Mathf.Lerp(wantedPosition.y, hit.point.y + walledCameraHeight, Time.deltaTime * damping);
                wantedPosition.z = hit.point.z + normal.z;
            }

            ActiveCamera.transform.position = wantedPosition;
            Vector3 lookPosition = player.TransformPoint(cameraHeightOffset);
            ActiveCamera.transform.LookAt(lookPosition);
        }
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

    public static void Clear() {
        currentY = player.parent.localEulerAngles.x;
        currentX = player.parent.localEulerAngles.y;
        RefreshCamera();
    }

}
