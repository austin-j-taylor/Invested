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

    private static Camera firstPersonCamera;
    private static Transform playerBody;
    private static Transform playerLookAtTarget;
    private static Transform externalPositionTarget; // Assigned by another part of the program for tracking
    private static Transform externalLookAtTarget; // Assigned by another part of the program for tracking

    private static bool cameraIsLocked;

    public static bool HasNotMovedCamera {
        get {
            return ActiveCamera.transform.localEulerAngles == Vector3.zero;
        }
    }

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
        firstPersonCamera = Camera.main;
        ActiveCamera = firstPersonCamera;
        Clear();
        UnlockCamera();
    }

    private void LateUpdate() {
        transform.position = playerBody.transform.position;

        if (externalPositionTarget) {
            if (cameraIsLocked) {
                // Called when the Camera is being controlled by some other source, i.e. HarmonyTarget
                ActiveCamera.transform.position = Vector3.Lerp(ActiveCamera.transform.position, externalPositionTarget.position, lerpConstant * Time.deltaTime);
                ActiveCamera.transform.LookAt(externalLookAtTarget);
            }
        } else {
            UpdateCamera();
        }
    }

    public static void UpdateCamera() {
        if (!externalPositionTarget) {
            ActiveCamera.transform.parent.localPosition = Vector3.zero;

            Vector3[] cameraPoints = new Vector3[9];
            cameraPoints[0] = ActiveCamera.ViewportToWorldPoint(new Vector3(0, 0, ActiveCamera.nearClipPlane));
            cameraPoints[1] = ActiveCamera.ViewportToWorldPoint(new Vector3(0, .5f, ActiveCamera.nearClipPlane));
            cameraPoints[2] = ActiveCamera.ViewportToWorldPoint(new Vector3(0, 1, ActiveCamera.nearClipPlane));
            cameraPoints[3] = ActiveCamera.ViewportToWorldPoint(new Vector3(.5f, 0, ActiveCamera.nearClipPlane));
            cameraPoints[4] = ActiveCamera.ViewportToWorldPoint(new Vector3(.5f, .5f, ActiveCamera.nearClipPlane));
            cameraPoints[5] = ActiveCamera.ViewportToWorldPoint(new Vector3(.5f, 1, ActiveCamera.nearClipPlane));
            cameraPoints[6] = ActiveCamera.ViewportToWorldPoint(new Vector3(1, 0, ActiveCamera.nearClipPlane));
            cameraPoints[7] = ActiveCamera.ViewportToWorldPoint(new Vector3(1, .5f, ActiveCamera.nearClipPlane));
            cameraPoints[8] = ActiveCamera.ViewportToWorldPoint(new Vector3(1, 1, ActiveCamera.nearClipPlane));
            
            // Check if lookAtTarget would be clipping into a ceiling
            Vector3[] playerDestinations = new Vector3[9];
            for (int i = 0; i < 9; i++) {
                playerDestinations[i] = cameraPoints[i] - playerLookAtTarget.localPosition;
            }

            int smallestIndex = -1;
            RaycastHit smallestHit = new RaycastHit();
            float smallestDistance = playerLookAtTarget.localPosition.y;

            for (int i = 0; i < 9; i++) {
                // Check height of lookAtTarget
                if (Physics.Raycast(playerDestinations[i], Vector3.up, out RaycastHit hit, (cameraPoints[i] - playerDestinations[i]).magnitude, GameManager.Layer_IgnoreCamera)) {
                    float distance = (playerDestinations[i] - hit.point).magnitude;
                    Debug.DrawLine(playerDestinations[i], hit.point, Color.green);
                    if (distance < smallestDistance) {
                        smallestIndex = i;
                        smallestHit = hit;
                        smallestDistance = distance;
                    }
                }
            }

            if (smallestIndex > -1) { // A collision has occured
                Vector3 height = smallestHit.point + (ActiveCamera.transform.position - cameraPoints[smallestIndex]) - ActiveCamera.transform.localPosition - new Vector3(0, .00001f, 0);
                height.x = 0;
                height.z = 0;
                ActiveCamera.transform.parent.position = height;
            }
        }
    }

    public static void Clear(bool resetRotation = true) {
        externalPositionTarget = null;
        externalLookAtTarget = null;
        firstPersonCamera.transform.localPosition = Vector3.zero;
        if (resetRotation) {
            Vector3 eulers = Player.PlayerInstance.transform.eulerAngles;
            eulers.x = 0;
            eulers.z = 0;
            playerLookAtTarget.rotation = Quaternion.Euler(eulers);
        }

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
        if (position == null) {
            externalPositionTarget = null;
            externalLookAtTarget = null;
            LockCamera();
        } else {
            externalPositionTarget = position;
            externalLookAtTarget = lookAt;
        }
    }

    public void SetThirdPerson() {
        Clear(false);
    }

    public void SetFirstPerson() {
        Clear(false);
    }
}
