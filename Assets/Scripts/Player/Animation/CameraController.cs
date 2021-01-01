using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the first- and third-person cameras.
/// </summary>
public class CameraController : MonoBehaviour {

    #region constants
    private const float rootConstantScaling = .5f;
    private const float stretchingVelocityFactor = -100;
    private const float cameraStretchingLerpFactor = 7;
    private static readonly Vector3 cameraControllerOffsetThirdPerson = new Vector3(0, 0.25f - .07f, 0);
    private static readonly Vector3 cameraControllerOffsetFirstPerson = new Vector3(0, .13f, 0);
    #endregion

    #region properties
    public static bool IsFirstPerson => SettingsMenu.settingsGameplay.cameraFirstPerson == 1;
    public static Camera ActiveCamera { get; private set; }
    public static Transform CameraLookAtTarget { get; private set; }
    public static Transform CameraPositionTarget { get; private set; }
    public static CinemachineBrain Cinemachine { get; set; }
    public static CinemachineVirtualCamera CinemachineThirdPersonCamera { get; set; }

    public static bool HasNotMovedCamera => currentX == startX && currentY == startY;
    public static bool UpsideDown => currentY < -89.99f || currentY > 89.99f;
    public static float Pitch => currentY;

    // Returns the horizontal direction the camera is facing (only in the x/z plane)
    public static Quaternion CameraDirection {
        get {
            Vector3 eulers = ActiveCamera.transform.eulerAngles;
            eulers.x = 0;
            eulers.z = 0;
            return Quaternion.Euler(eulers);
        }
    }
    #endregion

    #region fields
    private static CameraController instance;
    private static Transform playerBody;
    private static Transform playerCameraController;
    private static float CameraLookAtTargetOffset = 1;// offset from player to lookat target
    private static float currentX = 0;
    private static float currentY = 0;
    private static float startX = 0;
    private static float startY = 0;
    private static float lastCameraDistance = 0;
    //private static float stretchedOut = 0; // if above 0, the camera will "stretch out" further backwards.
    private static bool cameraIsLocked;
    #endregion

    void Awake() {
        instance = this;
        playerCameraController = transform;
        playerBody = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        CameraLookAtTarget = transform.Find("CameraLookAtTarget");
        CameraPositionTarget = CameraLookAtTarget.Find("CameraPositionTarget");
        ActiveCamera = CameraPositionTarget.Find("Camera").GetComponent<Camera>();
        ActiveCamera.depthTextureMode = DepthTextureMode.DepthNormals;
        Cinemachine = ActiveCamera.GetComponent<CinemachineBrain>();
        CinemachineThirdPersonCamera = CameraPositionTarget.GetComponentInChildren<CinemachineVirtualCamera>();
        SceneManager.sceneLoaded += ClearAfterSceneChange;
    }

    public static void Clear() {
        lastCameraDistance = 0;
        ActiveCamera.transform.localPosition = Vector3.zero;
        ActiveCamera.transform.localRotation = Quaternion.identity;
        Player.PlayerTransparancy.SetOverrideHidden(IsFirstPerson);

        if (Player.PlayerIronSteel.IsBurning) // Update blue lines when the camera is reset
            Player.PlayerIronSteel.UpdateBlueLines();
        UpdateCamera();
    }
    private void ClearAfterSceneChange(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Single) {
            if (scene.buildIndex == SceneSelectMenu.sceneTitleScreen) {
                UnlockCamera();
                ActiveCamera.clearFlags = CameraClearFlags.Skybox;
                Cinemachine.m_IgnoreTimeScale = true;
            } else {
                GameManager.SetCameraState(GameManager.GameCameraState.Standard);
                Cinemachine.m_IgnoreTimeScale = false;
                LockCamera();
                ActiveCamera.clearFlags = CameraClearFlags.Skybox;
            }
        }
    }
    #region updatingCamera
    private void LateUpdate() {
        if (cameraIsLocked && GameManager.CameraState == GameManager.GameCameraState.Standard) {
            float deltaX;
            float deltaY;
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) {
                deltaX = Input.GetAxis("HorizontalRight") * SettingsMenu.settingsGameplay.gamepadSensitivityX;
                deltaY = Input.GetAxis("VerticalRight") * SettingsMenu.settingsGameplay.gamepadSensitivityY;
            } else {
                deltaX = Input.GetAxis("Mouse X") * SettingsMenu.settingsGameplay.mouseSensitivityX;
                deltaY = Input.GetAxis("Mouse Y") * SettingsMenu.settingsGameplay.mouseSensitivityY;
            }
            // deltaY is normally the negative of the above statements, so an uninverted camera should be negatative
            if (SettingsMenu.settingsGameplay.cameraInvertX == 1)
                deltaX = -deltaX;
            if (SettingsMenu.settingsGameplay.cameraInvertY == 0)
                deltaY = -deltaY;
            if (SettingsMenu.settingsGameplay.cameraClamping == 1) {
                currentX += deltaX;
                currentY += deltaY;
                ClampY();
            } else {
                // If camera is upside-down, invert X controls, invert camera target
                currentY += deltaY;
                ModY();
                if (UpsideDown) {
                    currentX -= deltaX;
                } else {
                    currentX += deltaX;
                }

            }
        }
        UpdateCamera();
    }

    // Set camera position and rotation to follow the player for this frame.
    public static void UpdateCamera() {
        CameraLookAtTarget.rotation = Quaternion.Euler(0, currentX, 0);
        if (GameManager.CameraState == GameManager.GameCameraState.Standard)
            CinemachineThirdPersonCamera.transform.localPosition = Vector3.zero;

        /* Reset camera properties for this frame */
        float lastScale = playerCameraController.localScale.x;
        playerCameraController.localScale = new Vector3(1, 1, 1);

        // Vertical rotation (rotates camera up and down body)
        Quaternion verticalRotation = Quaternion.Euler(currentY, 0, 0);

        CameraPositionTarget.transform.localRotation = verticalRotation;
        //// If an external target was recently used, the camera ROTATION should lerp back
        //if (timeToLerp < maxTimeToLerp) {
        //    ActiveCamera.transform.localRotation = Quaternion.Slerp(ActiveCamera.transform.localRotation, verticalRotation, lerpConstantRotation * Time.unscaledDeltaTime);
        //} else {
        //    ActiveCamera.transform.localRotation = verticalRotation;
        //}

        if (IsFirstPerson) {
            playerCameraController.position = playerBody.position + cameraControllerOffsetFirstPerson;
            Vector3 wantedPosition = playerCameraController.position;

            CameraPositionTarget.transform.position = wantedPosition;
        } else {
            playerCameraController.position = playerBody.position + cameraControllerOffsetThirdPerson;

            // If the player is moving quickly, the camera stretches outwards.
            float cameraDistance = -SettingsMenu.settingsGameplay.cameraDistance;
            if (SettingsMenu.settingsGraphics.velocityZoom == 1) {
                cameraDistance = (2 - Mathf.Exp(Player.PlayerIronSteel.rb.velocity.sqrMagnitude / stretchingVelocityFactor)) * cameraDistance;
                if (lastCameraDistance != 0)
                    cameraDistance = Mathf.Lerp(lastCameraDistance, cameraDistance, Time.deltaTime * cameraStretchingLerpFactor);
            }
            Vector3 wantedPosition = verticalRotation * new Vector3(0, 0, cameraDistance);

            lastCameraDistance = cameraDistance;
            //// Decide position the camera should try to be at
            //Vector3 wantedPosition; // local
            //// If an external target was recently used, the camera POSITION should lerp back
            //if (timeToLerp < maxTimeToLerp) {
            //    float distance = timeToLerp / maxTimeToLerp;
            //    wantedPosition = Vector3.Slerp(ActiveCamera.transform.localPosition, verticalRotation * new Vector3(0, 0, -SettingsMenu.settingsGameplay.cameraDistance), lerpConstantPosition * Time.unscaledDeltaTime);
            //    timeToLerp += Time.unscaledDeltaTime;
            //} else { // normal operation
            //    wantedPosition = verticalRotation * new Vector3(0, 0, -SettingsMenu.settingsGameplay.cameraDistance);
            //}

            CameraPositionTarget.transform.localPosition = wantedPosition;
            //    Vector3 pos = Vector3.zero;
            //    pos.y = CameraLookAtTargetHeight;
            CameraLookAtTargetOffset = SettingsMenu.settingsGameplay.cameraDistance / 5;
            CameraLookAtTarget.transform.localPosition = new Vector3(0, CameraLookAtTargetOffset, 0);
            if (UpsideDown)
                CameraLookAtTarget.transform.localPosition = -CameraLookAtTarget.transform.localPosition;

            if (GameManager.CameraState == GameManager.GameCameraState.Standard) {
                // Checking for camera collision with walls
                // If in Cinemachine and we're a sufficiently far distance away, 
                // Raycast from player to camera and from camera to player.
                // In the event of a collision, scale THIS in all dimensions by (length from player to hit / length of camera distance)

                // The destinationsCamera[] are at the 9 corners of the camera in world space (top-left to center-center bottom-right)
                Vector3[] destinationsCamera = new Vector3[9];
                destinationsCamera[0] = ActiveCamera.ViewportToWorldPoint(new Vector3(0, 0, ActiveCamera.nearClipPlane));
                destinationsCamera[1] = ActiveCamera.ViewportToWorldPoint(new Vector3(0, .5f, ActiveCamera.nearClipPlane));
                destinationsCamera[2] = ActiveCamera.ViewportToWorldPoint(new Vector3(0, 1, ActiveCamera.nearClipPlane));
                destinationsCamera[3] = ActiveCamera.ViewportToWorldPoint(new Vector3(.5f, 0, ActiveCamera.nearClipPlane));
                destinationsCamera[4] = ActiveCamera.ViewportToWorldPoint(new Vector3(.5f, .5f, ActiveCamera.nearClipPlane));
                destinationsCamera[5] = ActiveCamera.ViewportToWorldPoint(new Vector3(.5f, 1, ActiveCamera.nearClipPlane));
                destinationsCamera[6] = ActiveCamera.ViewportToWorldPoint(new Vector3(1, 0, ActiveCamera.nearClipPlane));
                destinationsCamera[7] = ActiveCamera.ViewportToWorldPoint(new Vector3(1, .5f, ActiveCamera.nearClipPlane));
                destinationsCamera[8] = ActiveCamera.ViewportToWorldPoint(new Vector3(1, 1, ActiveCamera.nearClipPlane));

                // used for later
                Vector3 halfCameraHeight = destinationsCamera[0] - destinationsCamera[3];


                Vector3 originPlayer = playerCameraController.position;
                Vector3 destinationCamera = ActiveCamera.ViewportToWorldPoint(new Vector3(.5f, .5f, ActiveCamera.nearClipPlane));
                float length = Vector3.Distance(originPlayer, destinationCamera);

                Vector3 fromCameraToPlayer = originPlayer - destinationsCamera[4]; // center
                float distanceFromCameraToPlayer = Vector3.Distance(originPlayer, destinationsCamera[4]);

                if (distanceFromCameraToPlayer < -cameraDistance + 1) {
                    Vector3[] originsPlayer = new Vector3[9];
                    for (int i = 0; i < 9; i++) {
                        originsPlayer[i] = destinationsCamera[i] + fromCameraToPlayer;
                        Debug.DrawLine(originsPlayer[i], destinationsCamera[i], Color.gray);
                    }


                    //RaycastHit hit;
                    //if(Physics.Raycast(originPlayer, destinationCamera - originPlayer, out hit, length, GameManager.Layer_IgnoreCamera)) {
                    //    Debug.DrawLine(originPlayer, hit.point, Color.blue);
                    //    float scale = (hit.point - originPlayer).magnitude / length;
                    //    playerCameraController.localScale = new Vector3(scale, scale, scale);

                    //} else if (Physics.Raycast(destinationCamera, originPlayer - destinationCamera, out hit, length, GameManager.Layer_IgnoreCamera)) {
                    //    Debug.DrawLine(originPlayer, hit.point, Color.red);
                    //    float scale = (hit.point - originPlayer).magnitude / length;
                    //    playerCameraController.localScale = new Vector3(scale, scale, scale);

                    //} else {
                    //     If we were Colliding, LERP back to wanted position
                    //     If we weren't Colliding, just move camera instantly
                    //}

                    int smallestIndex = -1;
                    RaycastHit smallestHit = new RaycastHit();
                    float smallestDistance = Mathf.Infinity;

                    // check if the camera is angled such that it would clip into the ceiling
                    for (int i = 0; i < 9; i++) {
                        if (Physics.Raycast(originsPlayer[i], -fromCameraToPlayer, out RaycastHit hit, distanceFromCameraToPlayer, GameManager.Layer_IgnoreCamera)) {
                            float thisDistance = (hit.point - originsPlayer[i]).magnitude;
                            Debug.DrawLine(originsPlayer[i], hit.point, Color.yellow);
                            if (thisDistance < smallestDistance) {
                                smallestIndex = i;
                                smallestHit = hit;
                                smallestDistance = thisDistance;
                            }
                        }
                    }


                    float scale = 1;
                    if (smallestIndex > -1) { // A collision has occured
                        scale = (smallestHit.point - originsPlayer[smallestIndex]).magnitude / length;

                        if (scale > lastScale) {
                            // Before we were colliding with a closer wall, so "lerp" the scale to the further back one to be kind.
                            float newScale = Mathf.Pow(lastScale, rootConstantScaling);
                            if (newScale < scale) {
                                scale = newScale;
                            } else {
                                // shorten smallestHit.point to bring it closer to the player?
                            }
                        }

                        playerCameraController.localScale = new Vector3(scale, scale, scale);
                        Debug.DrawLine(originsPlayer[smallestIndex], smallestHit.point, Color.red);

                        // There is a small offset due to rescaling/rotation being relative to the CameraLookAtTarget, but the raycasts are to .25 above the player
                        // This brings the camera to be perfectly touching the smallest hit point
                        Vector3 offset;
                        switch (smallestIndex) {
                            case 0:
                                offset = ActiveCamera.ViewportToWorldPoint(new Vector3(0, 0, ActiveCamera.nearClipPlane));
                                break;
                            case 1:
                                offset = ActiveCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, ActiveCamera.nearClipPlane));
                                break;
                            case 2:
                                offset = ActiveCamera.ViewportToWorldPoint(new Vector3(0, 1, ActiveCamera.nearClipPlane));
                                break;
                            case 3:
                                offset = ActiveCamera.ViewportToWorldPoint(new Vector3(.5f, 0, ActiveCamera.nearClipPlane));
                                break;
                            case 4:
                                offset = ActiveCamera.ViewportToWorldPoint(new Vector3(.5f, .5f, ActiveCamera.nearClipPlane));
                                break;
                            case 5:
                                offset = ActiveCamera.ViewportToWorldPoint(new Vector3(.5f, 1, ActiveCamera.nearClipPlane));
                                break;
                            case 6:
                                offset = ActiveCamera.ViewportToWorldPoint(new Vector3(1, 0, ActiveCamera.nearClipPlane));
                                break;
                            case 7:
                                offset = ActiveCamera.ViewportToWorldPoint(new Vector3(1, .5f, ActiveCamera.nearClipPlane));
                                break;
                            default:
                                offset = ActiveCamera.ViewportToWorldPoint(new Vector3(1, 1, ActiveCamera.nearClipPlane));
                                break;
                        }
                        playerCameraController.localPosition += (smallestHit.point - offset);

                    } else if (lastScale < .98f) { // fuzzy == 1
                                                   // No collision.
                                                   // The camera is "recovering" back to a farther position. LERP to it instead.
                        scale = Mathf.Pow(lastScale, rootConstantScaling);
                        playerCameraController.localScale = new Vector3(scale, scale, scale);
                    }
                }
            }
        }

        //// If the camera's pitch is exactly 0, the nature of the VolumetricLines shader makes the blue lines to metals invisible.
        //// so, if it's exactly 0, give it a nudge.
        //if(CameraPositionTarget.transform.localEulerAngles.x == 0) {
        //    Debug.Log("zeroed");
        //    Vector3 angles = CameraPositionTarget.transform.localEulerAngles;
        //    angles.y = 0.00001f;
        //    CameraPositionTarget.transform.localEulerAngles = angles;
        //}
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
    #endregion

    public static void SetRotation(Vector3 eulers) {
        //eulers.x = 0;
        //eulers.z = 0;
        CameraLookAtTarget.rotation = Quaternion.Euler(eulers);

        currentX = CameraLookAtTarget.localEulerAngles.y;
        currentY = CameraLookAtTarget.localEulerAngles.x;
        startX = currentX;
        startY = currentY;
        ModY();
    }

    #region cameraControls
    public static void SetCinemachineCamera(CinemachineVirtualCamera vcam) {
        Player.PlayerTransparancy.SetOverrideHidden(false);
        GameManager.SetCameraState(GameManager.GameCameraState.Cutscene);
        vcam.enabled = true;

    }
    public static void DisableCinemachineCamera(CinemachineVirtualCamera vcam) {
        vcam.enabled = false;
        LockCamera();
        instance.StartCoroutine(instance.BlendOutOfCinemachineCamera());
    }
    private IEnumerator BlendOutOfCinemachineCamera() {
        yield return null;
        while (Cinemachine.IsBlending)
            yield return null;
        Player.PlayerTransparancy.SetOverrideHidden(IsFirstPerson);
        GameManager.SetCameraState(GameManager.GameCameraState.Standard);
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

    public void SetThirdPerson() {
        CameraPositionTarget.transform.SetParent(CameraLookAtTarget);
        if (GameManager.CameraState == GameManager.GameCameraState.Standard) {
            Player.PlayerTransparancy.SetOverrideHidden(false);
            Clear();
        }
    }
    public void SetFirstPerson() {
        CameraPositionTarget.transform.SetParent(CameraLookAtTarget.Find("FirstPersonTarget"));
        if (GameManager.CameraState == GameManager.GameCameraState.Standard) {
            Player.PlayerTransparancy.SetOverrideHidden(true);
            Clear();
        }
    }

    public static void TogglePerspective() {
        if (IsFirstPerson) {
            instance.SetThirdPerson();
            SettingsMenu.RefreshSettingPerspective(0);
        } else {
            instance.SetFirstPerson();
            SettingsMenu.RefreshSettingPerspective(1);
        }
    }
    #endregion
}
