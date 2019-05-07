using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Controls the first- and third-person cameras.
 * Adapted from http://wiki.unity3d.com/index.php?title=SmoothFollowWithCameraBumper
 */

public class CameraController : MonoBehaviour {

    private const float playerLookAtTargetReferenceHeight = 1.25f;
    private const float playerLookAtTargetFirstPersonHeight = 1;
    private const float distanceFromHitWall = .125f;//.5f;
    private const float lerpConstantPosition = 5;
    private const float lerpConstantRotation = 15;
    public static Camera ActiveCamera { get; private set; }

    private static Camera thirdPersonCamera;
    private static Camera firstPersonCamera;
    private static Transform playerBody;
    private static Transform playerLookAtTarget;
    private static Transform externalPositionTarget; // Assigned by another part of the program for tracking
    private static Transform externalLookAtTarget; // Assigned by another part of the program for tracking

    private static float currentX = 0;
    private static float currentY = 0;
    private static float playerLookAtTargetHeight = playerLookAtTargetReferenceHeight;
    private static bool cameraIsLocked;

    public static bool HasNotMovedCamera {
        get {
            return currentX == 0 && currentY == 0;
        }
    }
    public static bool UpsideDown {
        get {
            return currentY < -89.99f || currentY > 89.99f;
        }
    }
    // Assigned when the camera should lerp to its target, rather than immediately go to it
    public static float TimeToLerp {
        get {
            return timeToLerp;
        }
        set {
            timeToLerp = 0;
            maxTimeToLerp = value;
        }
    }
    private static float timeToLerp;
    private static float maxTimeToLerp;

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

        if (externalPositionTarget) {
            // Called when the Camera is being controlled by some other source, i.e. HarmonyTarget
            ActiveCamera.transform.position = Vector3.Lerp(ActiveCamera.transform.position, externalPositionTarget.position, lerpConstantPosition * Time.deltaTime);
            //ActiveCamera.transform.LookAt(externalLookAtTarget);
            ActiveCamera.transform.rotation = Quaternion.Lerp(ActiveCamera.transform.rotation, Quaternion.LookRotation(externalLookAtTarget.position - ActiveCamera.transform.position, Vector3.up), lerpConstantRotation * Time.deltaTime);
        } else {
            if (cameraIsLocked) {
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
                if (SettingsMenu.settingsData.cameraClamping == 1) {
                    currentX += deltaX;
                    currentY += deltaY;
                    ClampY();
                } else {
                    // If camera is upside-down, invert X controls, invert camera target
                    currentY += deltaY;
                    ModY();
                    if(UpsideDown) {
                        currentX -= deltaX;
                        playerLookAtTargetHeight = -playerLookAtTargetReferenceHeight;
                    } else {
                        currentX += deltaX;
                        playerLookAtTargetHeight = playerLookAtTargetReferenceHeight;
                    }

                }
            }
            UpdateCamera();
        }
    }

    public static void UpdateCamera() {
        if (!externalPositionTarget) {
            // Horizontal rotation (rotates playerBody body left and right)
            // Vertical rotation (rotates camera up and down body)
            Quaternion verticalRotation = Quaternion.Euler(currentY, 0, 0);

            // If an external target was recently used, the camera should lerp back
            if (timeToLerp < maxTimeToLerp) {
                ActiveCamera.transform.localRotation = Quaternion.Slerp(ActiveCamera.transform.localRotation, verticalRotation, lerpConstantRotation * Time.deltaTime);
            } else {
                ActiveCamera.transform.localRotation = verticalRotation;
            }

            if (SettingsMenu.settingsData.cameraFirstPerson == 0) {
                // Third person

                // Decide position the camera should try to be at
                Vector3 wantedPosition; // local
                // If an external target was recently used, the camera should lerp back
                if (timeToLerp < maxTimeToLerp) {
                    float distance = timeToLerp / maxTimeToLerp;
                    wantedPosition = Vector3.Slerp(ActiveCamera.transform.localPosition, verticalRotation * new Vector3(0, 0, -SettingsMenu.settingsData.cameraDistance), lerpConstantPosition * Time.deltaTime);
                    timeToLerp += Time.deltaTime;
                } else {
                    wantedPosition = verticalRotation * new Vector3(0, 0, -SettingsMenu.settingsData.cameraDistance);
                }

                ActiveCamera.transform.localPosition = wantedPosition;
                Vector3 pos = Vector3.zero;
                pos.y = playerLookAtTargetHeight;
                playerLookAtTarget.transform.localPosition = pos;

                Vector3[] origins = new Vector3[9];
                origins[0] = ActiveCamera.ViewportToWorldPoint(new Vector3(0, 0, ActiveCamera.nearClipPlane));
                origins[1] = ActiveCamera.ViewportToWorldPoint(new Vector3(0, .5f, ActiveCamera.nearClipPlane));
                origins[2] = ActiveCamera.ViewportToWorldPoint(new Vector3(0, 1, ActiveCamera.nearClipPlane));
                origins[3] = ActiveCamera.ViewportToWorldPoint(new Vector3(.5f, 0, ActiveCamera.nearClipPlane));
                origins[4] = ActiveCamera.ViewportToWorldPoint(new Vector3(.5f, .5f, ActiveCamera.nearClipPlane));
                origins[5] = ActiveCamera.ViewportToWorldPoint(new Vector3(.5f, 1, ActiveCamera.nearClipPlane));
                origins[6] = ActiveCamera.ViewportToWorldPoint(new Vector3(1, 0, ActiveCamera.nearClipPlane));
                origins[7] = ActiveCamera.ViewportToWorldPoint(new Vector3(1, .5f, ActiveCamera.nearClipPlane));
                origins[8] = ActiveCamera.ViewportToWorldPoint(new Vector3(1, 1, ActiveCamera.nearClipPlane));

                Vector3 directionToTarget = playerLookAtTarget.position - origins[4]; // center
                float distanceToTarget = directionToTarget.magnitude;

                Vector3[] destinations = new Vector3[9];
                for (int i = 0; i < 9; i++)
                    destinations[i] = origins[i] + directionToTarget;

                // Check if lookAtTarget would be clipping into a ceiling
                Vector3[] playerDestinations = new Vector3[9];
                for (int i = 0; i < 9; i++) {
                    playerDestinations[i] = destinations[i] - playerLookAtTarget.localPosition;
                    Debug.DrawLine(playerDestinations[i], destinations[i], Color.white);
                }

                int smallestIndex = -1;
                RaycastHit smallestHit = new RaycastHit();
                float smallestDistance = playerLookAtTargetReferenceHeight;

                for (int i = 0; i < 9; i++) {
                    // Check height of lookAtTarget
                    if (Physics.Raycast(playerDestinations[i], UpsideDown ? Vector3.down : Vector3.up, out RaycastHit hit, (destinations[i] - playerDestinations[i]).magnitude, GameManager.Layer_IgnoreCamera)) {
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
                    playerLookAtTarget.position = smallestHit.point + (playerLookAtTarget.position - destinations[smallestIndex]) + (UpsideDown ? new Vector3(0, .00001f, 0) : new Vector3(0, -.00001f, 0));
                    Debug.DrawLine(playerDestinations[smallestIndex], smallestHit.point, Color.red);

                    // Recalculate based on new lookAtTarget position
                    ActiveCamera.transform.localPosition = wantedPosition;
                    origins[0] = ActiveCamera.ViewportToWorldPoint(new Vector3(0, 0, ActiveCamera.nearClipPlane));
                    origins[1] = ActiveCamera.ViewportToWorldPoint(new Vector3(0, .5f, ActiveCamera.nearClipPlane));
                    origins[2] = ActiveCamera.ViewportToWorldPoint(new Vector3(0, 1, ActiveCamera.nearClipPlane));
                    origins[3] = ActiveCamera.ViewportToWorldPoint(new Vector3(.5f, 0, ActiveCamera.nearClipPlane));
                    origins[4] = ActiveCamera.ViewportToWorldPoint(new Vector3(.5f, .5f, ActiveCamera.nearClipPlane));
                    origins[5] = ActiveCamera.ViewportToWorldPoint(new Vector3(.5f, 1, ActiveCamera.nearClipPlane));
                    origins[6] = ActiveCamera.ViewportToWorldPoint(new Vector3(1, 0, ActiveCamera.nearClipPlane));
                    origins[7] = ActiveCamera.ViewportToWorldPoint(new Vector3(1, .5f, ActiveCamera.nearClipPlane));
                    origins[8] = ActiveCamera.ViewportToWorldPoint(new Vector3(1, 1, ActiveCamera.nearClipPlane));

                    directionToTarget = playerLookAtTarget.position - origins[4]; // center
                    distanceToTarget = directionToTarget.magnitude;

                    for (int i = 0; i < 9; i++) {
                        destinations[i] = origins[i] + directionToTarget;
                    }
                }

                smallestIndex = -1;
                smallestDistance = SettingsMenu.settingsData.cameraDistance;

                for (int i = 0; i < 9; i++) {
                    if (Physics.Raycast(destinations[i], -directionToTarget, out RaycastHit hit, distanceToTarget, GameManager.Layer_IgnoreCamera)) {
                        float distance = (hit.point - destinations[i]).magnitude;
                        Debug.DrawLine(destinations[i], hit.point, Color.yellow);
                        if (distance < smallestDistance) {
                            smallestIndex = i;
                            smallestHit = hit;
                            smallestDistance = distance;
                        }
                    }
                }

                if (smallestIndex > -1) { // A collision has occured
                    ActiveCamera.transform.position = smallestHit.point + (ActiveCamera.transform.position - origins[smallestIndex]);
                    Debug.DrawLine(destinations[smallestIndex], smallestHit.point, Color.red);
                }
            } else {
                // First person
                // Decide position the camera should try to be at
                Vector3 wantedPosition = Vector3.zero; // local
                // If an external target was recently used, the camera should lerp back
                if (timeToLerp < maxTimeToLerp) {
                    float distance = timeToLerp / maxTimeToLerp;
                    wantedPosition = Vector3.Slerp(ActiveCamera.transform.localPosition, Vector3.zero, lerpConstantPosition * Time.deltaTime);
                    timeToLerp += Time.deltaTime;

                    ActiveCamera.transform.localPosition = wantedPosition;
                } else {

                }

                Vector3 pos = Vector3.zero;
                pos.y = playerLookAtTargetFirstPersonHeight;
                playerLookAtTarget.transform.localPosition = pos;

                Vector3[] origins = new Vector3[9];
                origins[0] = ActiveCamera.ViewportToWorldPoint(new Vector3(0, 0, ActiveCamera.nearClipPlane));
                origins[1] = ActiveCamera.ViewportToWorldPoint(new Vector3(0, .5f, ActiveCamera.nearClipPlane));
                origins[2] = ActiveCamera.ViewportToWorldPoint(new Vector3(0, 1, ActiveCamera.nearClipPlane));
                origins[3] = ActiveCamera.ViewportToWorldPoint(new Vector3(.5f, 0, ActiveCamera.nearClipPlane));
                origins[4] = ActiveCamera.ViewportToWorldPoint(new Vector3(.5f, .5f, ActiveCamera.nearClipPlane));
                origins[5] = ActiveCamera.ViewportToWorldPoint(new Vector3(.5f, 1, ActiveCamera.nearClipPlane));
                origins[6] = ActiveCamera.ViewportToWorldPoint(new Vector3(1, 0, ActiveCamera.nearClipPlane));
                origins[7] = ActiveCamera.ViewportToWorldPoint(new Vector3(1, .5f, ActiveCamera.nearClipPlane));
                origins[8] = ActiveCamera.ViewportToWorldPoint(new Vector3(1, 1, ActiveCamera.nearClipPlane));

                Vector3 directionToTarget = playerLookAtTarget.position - origins[4]; // center
                float distanceToTarget = directionToTarget.magnitude;

                Vector3[] destinations = new Vector3[9];
                for (int i = 0; i < 9; i++)
                    destinations[i] = origins[i] + directionToTarget;

                // Check if lookAtTarget would be clipping into a ceiling
                Vector3[] playerDestinations = new Vector3[9];
                for (int i = 0; i < 9; i++) {
                    playerDestinations[i] = destinations[i] - playerLookAtTarget.localPosition;
                }

                int smallestIndex = -1;
                RaycastHit smallestHit = new RaycastHit();
                float smallestDistance = playerLookAtTargetHeight;

                for (int i = 0; i < 9; i++) {
                    // Check height of lookAtTarget
                    if (Physics.Raycast(playerDestinations[i], Vector3.up, out RaycastHit hit, (destinations[i] - playerDestinations[i]).magnitude, GameManager.Layer_IgnoreCamera)) {
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
                    playerLookAtTarget.position = smallestHit.point + (playerLookAtTarget.position - destinations[smallestIndex]) - new Vector3(0, .00001f, 0);
                }
            }
        }
    }

    public static void Clear(bool resetRotation = true) {
        playerLookAtTargetHeight = playerLookAtTargetReferenceHeight;
        TimeToLerp = -100;
        externalPositionTarget = null;
        externalLookAtTarget = null;
        firstPersonCamera.transform.localPosition = Vector3.zero;
        if(resetRotation) {
            Vector3 eulers = Player.PlayerInstance.transform.eulerAngles;
            eulers.x = 0;
            eulers.z = 0;
            playerLookAtTarget.rotation = Quaternion.Euler(eulers);

            currentY = playerLookAtTarget.localEulerAngles.x;
            currentX = playerLookAtTarget.localEulerAngles.y;
            ModY();
        }

        if (Player.PlayerIronSteel.IsBurning) // Update blue lines when the camera is reset
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

    private void ClampY() {
        currentY = Mathf.Clamp(currentY, -89.99f, 89.99f); // (controls top, controls bottom)
    }
    private static void ModY() {
        if (currentY >= 270)
            currentY -= 360;
        else if (currentY <= -270)
            currentY += 360;
    }

    public void SetThirdPerson() {
        firstPersonCamera.gameObject.SetActive(false);
        thirdPersonCamera.gameObject.SetActive(true);
        thirdPersonCamera.cullingMask = firstPersonCamera.cullingMask;
        ActiveCamera = thirdPersonCamera;
        Clear(false);
    }

    public void SetFirstPerson() {
        thirdPersonCamera.gameObject.SetActive(false);
        firstPersonCamera.gameObject.SetActive(true);
        firstPersonCamera.cullingMask = thirdPersonCamera.cullingMask;
        ActiveCamera = firstPersonCamera;
        Clear(false);
    }
}
