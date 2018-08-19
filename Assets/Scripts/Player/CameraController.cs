using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Controls the first-person camera.
 */

public class CameraController : MonoBehaviour {

    public static Camera ActiveCamera { get; private set; }

    public static Camera thirdPersonCamera;
    public static Camera firstPersonCamera;
    private static Transform player;
    private static Vector2 cameraPosition;
    private static Vector2 cameraVelocity;
    private readonly static Vector2 gamepadSpeed = new Vector3(80, 64);

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
    public static float Sensitivity { get; set; } = 2f;
    public static float Smoothing { get; set; } = 1f;

    // Use this for initialization
    void Awake() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        firstPersonCamera = player.Find("Body/FirstPersonCamera").GetComponent<Camera>();
        thirdPersonCamera = player.Find("Body/ThirdPersonCamera").GetComponent<Camera>();
        FirstPerson = false;
        Clear();
        UnlockCamera();
    }

    // Update is called once per frame
    void Update() {
        if (CameraIsLocked) {
            if (GamepadController.UsingGamepad) {
                Vector2 gamepadInput = new Vector2(Input.GetAxis("HorizontalRight"), Input.GetAxis("VerticalRight"));
                gamepadInput.x *= gamepadSpeed.x * Sensitivity;
                gamepadInput.y *= gamepadSpeed.y * Sensitivity;
                gamepadInput *= Time.deltaTime;

                cameraVelocity.x = Mathf.Lerp(cameraVelocity.x, gamepadInput.x, 1f / Smoothing);
                cameraVelocity.y = Mathf.Lerp(cameraVelocity.y, gamepadInput.y, 1f / Smoothing);
            } else {
                Vector2 mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
                mouseInput = Vector2.Scale(mouseInput, new Vector2(Sensitivity, Sensitivity));

                cameraVelocity.x = Mathf.Lerp(cameraVelocity.x, mouseInput.x, 1f / Smoothing);
                cameraVelocity.y = Mathf.Lerp(cameraVelocity.y, mouseInput.y, 1f / Smoothing);
            }
            cameraPosition += cameraVelocity;
            cameraPosition.y = Mathf.Clamp(cameraPosition.y, -90f, 90f);

            ActiveCamera.transform.localRotation = Quaternion.AngleAxis(-cameraPosition.y, Vector3.right);
            player.localRotation = Quaternion.AngleAxis(cameraPosition.x, player.up);
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
        cameraPosition.x = player.eulerAngles.y;
        if (firstPerson) 
            cameraPosition.y = player.eulerAngles.x;
        else
            cameraPosition.y = player.eulerAngles.x - Mathf.Rad2Deg * Mathf.Atan2(ActiveCamera.transform.localPosition.y, -ActiveCamera.transform.localPosition.z);
    }

}
