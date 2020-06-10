using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Controls the first- and third-person cameras.
 * Also handles management of cloud shader data between scenes. Should change that.
 * Adapted from http://wiki.unity3d.com/index.php?title=SmoothFollowWithCameraBumper
 */

public class CameraController : MonoBehaviour {

    private const float CameraLookAtTargetFirstPersonHeight = 1;
    private const float rootConstantScaling = .5f;
    private const float stretchingVelocityFactor = -100;
    private const float cameraStretchingLerpFactor = 7;
    private static readonly Vector3 cameraControllerOffset = new Vector3(0, 0.25f - .06f, 0);

    public static Camera ActiveCamera { get; private set; }
    private static CameraController instance;

    private static Transform playerBody;
    private static Transform playerCameraController;
    public static Transform CameraLookAtTarget { get; private set; }
    public static Transform CameraPositionTarget { get; private set; }
    public static bool UsingCinemachine { get; set; } // Set to true when using Cinemachine animation. Do pretty much nothing in our Update().
    public static CinemachineBrain Cinemachine;

    private static float CameraLookAtTargetOffset = 1;// offset from player to lookat target
    private static float currentX = 0;
    private static float currentY = 0;
    private static float startX = 0;
    private static float startY = 0;
    private static float lastCameraDistance = 0;
    //private static float stretchedOut = 0; // if above 0, the camera will "stretch out" further backwards.
    private static bool cameraIsLocked;

    private static CloudMaster clouds; // perceives clouds on certain levels
    private static float cloudsMaxDensity; // used for fading between densities
    private static bool SceneUsesClouds;

    public static bool HasNotMovedCamera {
        get {
            return currentX == startX && currentY == startY;
        }
    }
    public static bool UpsideDown {
        get {
            return currentY < -89.99f || currentY > 89.99f;
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
        instance = this;
        playerCameraController = transform;
        playerBody = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        CameraLookAtTarget = transform.Find("CameraLookAtTarget");
        CameraPositionTarget = CameraLookAtTarget.Find("CameraPositionTarget");
        ActiveCamera = CameraPositionTarget.Find("Camera").GetComponent<Camera>();
        ActiveCamera.depthTextureMode = DepthTextureMode.DepthNormals;
        clouds = ActiveCamera.GetComponent<CloudMaster>();
        Cinemachine = ActiveCamera.GetComponent<CinemachineBrain>();
        UnlockCamera();
        SceneManager.sceneLoaded += LoadCloudDataFromScene;
    }

    private void LateUpdate() {

        //if (UsingCinemachine) {
        //    return;
        //}
        //GetComponentInChildren<CinemachineVirtualCamera>().transform.localRotation = Quaternion.identity;
        //GetComponentInChildren<CinemachineVirtualCamera>().transform.localPosition = Vector3.zero;


        if (cameraIsLocked && !UsingCinemachine) {
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
        instance.transform.position = playerBody.position + cameraControllerOffset;
        CameraLookAtTarget.rotation = Quaternion.Euler(0, currentX, 0);

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

        if (SettingsMenu.settingsData.cameraFirstPerson == 0) { // Third person

            // If the player is moving quickly, the camera stretches outwards.
            float cameraDistance = -(2 - Mathf.Exp(Player.PlayerIronSteel.rb.velocity.sqrMagnitude / stretchingVelocityFactor)) * SettingsMenu.settingsData.cameraDistance;
            if (lastCameraDistance != 0)
                cameraDistance = Mathf.Lerp(lastCameraDistance, cameraDistance, Time.deltaTime * cameraStretchingLerpFactor);
            Vector3 wantedPosition = verticalRotation * new Vector3(0, 0, cameraDistance);

            lastCameraDistance = cameraDistance;
            //// Decide position the camera should try to be at
            //Vector3 wantedPosition; // local
            //// If an external target was recently used, the camera POSITION should lerp back
            //if (timeToLerp < maxTimeToLerp) {
            //    float distance = timeToLerp / maxTimeToLerp;
            //    wantedPosition = Vector3.Slerp(ActiveCamera.transform.localPosition, verticalRotation * new Vector3(0, 0, -SettingsMenu.settingsData.cameraDistance), lerpConstantPosition * Time.unscaledDeltaTime);
            //    timeToLerp += Time.unscaledDeltaTime;
            //} else { // normal operation
            //    wantedPosition = verticalRotation * new Vector3(0, 0, -SettingsMenu.settingsData.cameraDistance);
            //}

            CameraPositionTarget.transform.localPosition = wantedPosition;
            //    Vector3 pos = Vector3.zero;
            //    pos.y = CameraLookAtTargetHeight;
            CameraLookAtTargetOffset = SettingsMenu.settingsData.cameraDistance / 5;
            CameraLookAtTarget.transform.localPosition = new Vector3(0, CameraLookAtTargetOffset, 0);
            if (UpsideDown)
                CameraLookAtTarget.transform.localPosition = -CameraLookAtTarget.transform.localPosition;

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


            Vector3 originPlayer = playerBody.position + cameraControllerOffset;
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
        } else { // first person
            Vector3 wantedPosition = playerCameraController.position;

            CameraPositionTarget.transform.position = wantedPosition;
        }
    }

    public static void Clear() {
        lastCameraDistance = 0;
        ActiveCamera.transform.localPosition = Vector3.zero;
        ActiveCamera.transform.localRotation = Quaternion.identity;
        UsingCinemachine = false;
        Player.PlayerTransparancy.SetOverrideHidden(SettingsMenu.settingsData.cameraFirstPerson == 1);

        if (Player.PlayerIronSteel.IsBurning) // Update blue lines when the camera is reset
            Player.PlayerIronSteel.UpdateBlueLines();
        UpdateCamera();
    }
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

    public static void SetCinemachineCamera(CinemachineVirtualCamera vcam) {
        UsingCinemachine = true;
        vcam.enabled = true;
    }
    public static void DisableCinemachineCamera(CinemachineVirtualCamera vcam) {
        UsingCinemachine = false;
        vcam.enabled = false;
        LockCamera();
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
        Player.PlayerTransparancy.SetOverrideHidden(false);
        CameraPositionTarget.transform.SetParent(CameraLookAtTarget);
        if (!UsingCinemachine)
            Clear();
    }
    public void SetFirstPerson() {
        Player.PlayerTransparancy.SetOverrideHidden(true);
        CameraPositionTarget.transform.SetParent(CameraLookAtTarget.Find("FirstPersonTarget"));
        if (!UsingCinemachine)
            Clear();
    }

    public static void TogglePerspective() {
        if (SettingsMenu.settingsData.cameraFirstPerson == 0) {
            instance.SetFirstPerson();
            SettingsMenu.RefreshSettingPerspective(1);
        } else {
            instance.SetThirdPerson();
            SettingsMenu.RefreshSettingPerspective(0);
        }
    }

    // On scene startup
    // Copy parameters from this scene's cloud controller to the camera
    private void LoadCloudDataFromScene(Scene scene, LoadSceneMode mode) {
        // mode is Single when it's loading scenes on startup, so skip those
        if (mode == LoadSceneMode.Single) {
            GameObject otherObject = GameObject.Find("Clouds");
            if (otherObject) {
                //ActiveCamera.clearFlags = CameraClearFlags.SolidColor;
                CloudMaster other = otherObject.GetComponent<CloudMaster>();

                SetCloudData(other);
            } else {
                // No clouds for this scene
                SceneUsesClouds = false;
                clouds.enabled = false;
                //ActiveCamera.clearFlags = CameraClearFlags.Skybox;
            }
        }
    }

    public static void SetClouds(bool enable) {
        if (enable) {
            if (SceneUsesClouds)
                clouds.enabled = true;
        } else {
            clouds.enabled = false;
        }
    }
    // Loads cloud settings from the passed CloudMaster.
    public static void SetCloudData(CloudMaster other) {
        clouds.shader = other.shader;
        clouds.container = other.container;
        clouds.weatherMapGen = other.weatherMapGen;
        clouds.noise = other.noise;
        clouds.sunLight = other.sunLight;
        clouds.cloudTestParams = other.cloudTestParams;

        clouds.numStepsLight = other.numStepsLight;
        clouds.rayOffsetStrength = other.rayOffsetStrength;
        clouds.especiallyNoisyRayOffsets = other.especiallyNoisyRayOffsets;
        clouds.blueNoise = other.blueNoise;

        clouds.cloudScale = other.cloudScale;
        clouds.densityMultiplier = other.densityMultiplier;
        clouds.densityOffset = other.densityOffset;
        clouds.shapeOffset = other.shapeOffset;
        clouds.heightOffset = other.heightOffset;
        clouds.shapeNoiseWeights = other.shapeNoiseWeights;

        clouds.detailNoiseScale = other.detailNoiseScale;
        clouds.detailNoiseWeight = other.detailNoiseWeight;
        clouds.detailNoiseWeights = other.detailNoiseWeights;
        clouds.detailOffset = other.detailOffset;

        clouds.lightAbsorptionThroughCloud = other.lightAbsorptionThroughCloud;
        clouds.lightAbsorptionTowardSun = other.lightAbsorptionTowardSun;
        clouds.darknessThreshold = other.darknessThreshold;
        clouds.forwardScattering = other.forwardScattering;
        clouds.backScattering = other.backScattering;
        clouds.baseBrightness = other.baseBrightness;
        clouds.phaseFactor = other.phaseFactor;

        clouds.timeScale = other.timeScale;
        clouds.baseSpeed = other.baseSpeed;
        clouds.detailSpeed = other.detailSpeed;

        clouds.colFog = other.colFog;
        clouds.colClouds = other.colClouds;
        clouds.colSun = other.colSun;
        clouds.haveSunInSky = other.haveSunInSky;
        clouds.fogDensity = other.fogDensity;

        clouds.cloudsFollowPlayerXZ = other.cloudsFollowPlayerXZ;
        clouds.cloudsFollowPlayerXYZ = other.cloudsFollowPlayerXYZ;

        clouds.material = other.material;

        SceneUsesClouds = true;
        clouds.enabled = GraphicsController.CloudsEnabled;
        clouds.Awake();

        other.enabled = false;

        cloudsMaxDensity = clouds.densityMultiplier;
    }

    public static void FadeCloudsIn(float time) {
        instance.StartCoroutine(instance.CloudsIn(time));
    }
    public static void FadeCloudsOut(float time) {
        instance.StartCoroutine(instance.CloudsOut(time));
    }
    public static void FadeCloudsOutImmediate() {
        clouds.densityMultiplier = 0;
    }
    private IEnumerator CloudsIn(float fadeTime) {
        float density = 0;
        if (cloudsMaxDensity < 0) {
            while (density > cloudsMaxDensity) {
                density += Time.deltaTime / fadeTime * cloudsMaxDensity;
                clouds.densityMultiplier = density;
                yield return null;
            }
        } else {
            while (density < cloudsMaxDensity) {
                density += Time.deltaTime / fadeTime * cloudsMaxDensity;
                clouds.densityMultiplier = density;
                yield return null;
            }
        }
        clouds.densityMultiplier = cloudsMaxDensity;
    }
    private IEnumerator CloudsOut(float fadeTime) {
        float density = cloudsMaxDensity;
        if (cloudsMaxDensity < 0) {
            while (density < 0) {
                density -= Time.deltaTime / fadeTime * cloudsMaxDensity;
                clouds.densityMultiplier = density;
                yield return null;
            }
        } else {
            while (density > 0) {
                density -= Time.deltaTime / fadeTime * cloudsMaxDensity;
                clouds.densityMultiplier = density;
                yield return null;
            }
        }
        clouds.densityMultiplier = 0;
    }
}
