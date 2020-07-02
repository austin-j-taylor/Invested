using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

// Handles the target that is at the end of the level.
public class HarmonyTarget : MonoBehaviour {

    private const float distanceThreshold = .001f;
    private const float distanceThresholdPulling = 10f;
    private const float forceConstantFar = 40f;
    private const float forceConstantClose = 10000f;
    private const float lerpConstant = 7;
    private const float timeToLerpBack = 2;
    private readonly Vector3 positionLeft = new Vector3(0, 0, 2.39419f);

    [SerializeField]
    private int numSpikes = 3;

    private bool playerHasEntered;
    private bool controllingPlayer;
    private Quaternion zeroRotation;

    private CinemachineVirtualCamera vcam;
    private Rigidbody rb;
    private Animator anim;
    private Renderer[] symbolRenderers;
    private Transform harmonySphere;
    private Transform inner;
    private Transform outerLeft;
    private Transform outerRight;
    private Transform spikeLeft;
    private Transform spikeCenter;
    private Transform spikeRight;
    private Transform cameraPositionTarget;
    private Transform cameraLookAtTarget;

    public bool Unlocked => numSpikes >= 3;

    private void Start() {
        vcam = GetComponentInChildren<CinemachineVirtualCamera>();
        vcam.enabled = false;

        rb = GetComponentInChildren<Rigidbody>();
        anim = GetComponent<Animator>();
        harmonySphere = rb.GetComponent<Transform>();
        cameraPositionTarget = transform.Find("CameraPositionAnchor/CameraPositionTarget");
        inner = transform.Find("Inner");
        spikeLeft = inner.Find("Spike_Left");
        spikeCenter = inner.Find("Spike_Center");
        spikeRight = inner.Find("Spike_Right");
        outerLeft = transform.Find("Outer_Left");
        outerRight = transform.Find("Outer_Right");
        cameraLookAtTarget = inner.Find("CameraLookAtTarget");

        symbolRenderers = GetComponentsInChildren<Renderer>();
        harmonySphere.gameObject.AddComponent<HarmonySphere>();

        playerHasEntered = false;
        controllingPlayer = false;
        switch (numSpikes) {
            case 3:
                spikeLeft.GetComponent<Renderer>().enabled = true;
                spikeCenter.GetComponent<Renderer>().enabled = true;
                spikeRight.GetComponent<Renderer>().enabled = true;
                anim.SetInteger("SpikeCount", numSpikes);
                break;
            case 2:
                spikeLeft.GetComponent<Renderer>().enabled = true;
                spikeCenter.GetComponent<Renderer>().enabled = false;
                spikeRight.GetComponent<Renderer>().enabled = true;
                anim.SetInteger("SpikeCount", numSpikes);
                break;
            case 1:
                spikeLeft.GetComponent<Renderer>().enabled = true;
                spikeCenter.GetComponent<Renderer>().enabled = false;
                spikeRight.GetComponent<Renderer>().enabled = false;
                anim.SetInteger("SpikeCount", numSpikes);
                break;
        }
        zeroRotation = transform.rotation;
    }

    private void LateUpdate() {
        if (!playerHasEntered) {
            // Rotate Target towards Player
            Vector3 distancetoPlayer = harmonySphere.position - CameraController.ActiveCamera.transform.position;// player.transform.position;
            float angle = 180 + Mathf.Atan2(distancetoPlayer.x, distancetoPlayer.z) * Mathf.Rad2Deg;
            Vector3 newRotation = Vector3.zero;
            ////angle = Mathf.LerpAngle(inner.eulerAngles.y, angle, Time.deltaTime * 10f);

            newRotation.y = angle;
            //transform.localEulerAngles = newRotation;
            outerLeft.localEulerAngles -= newRotation;
            outerRight.localEulerAngles -= newRotation;

            transform.LookAt(CameraController.ActiveCamera.transform.position);

        } else {
            transform.rotation = Quaternion.Slerp(transform.rotation, zeroRotation, Time.deltaTime * lerpConstant);
        }
    }

    private void FixedUpdate() {
        if (!playerHasEntered) {
            // Pull sphere towards center of Harmony Target
            Vector3 distance = inner.position - harmonySphere.position;
            float sqrDistance = distance.sqrMagnitude;
            if (sqrDistance > distanceThreshold) {
                rb.AddForce(forceConstantFar * distance.normalized * sqrDistance, ForceMode.Acceleration);
            }
        } else if (controllingPlayer) {
            // Pull player towards center of Harmony Target
            Vector3 distance = Player.PlayerIronSteel.CenterOfMass - inner.position;
            Player.PlayerIronSteel.GetComponent<Rigidbody>().AddForce(-forceConstantClose * distance, ForceMode.Force);
            Vector3 pos = cameraPositionTarget.position;
            pos.y = cameraLookAtTarget.position.y;
            cameraPositionTarget.position = pos;

            // So the light from the sphere doesn't flicker around the player's body
            harmonySphere.position = Player.PlayerIronSteel.CenterOfMass;
        }
    }

    private void BeginAnimation() {
        playerHasEntered = true;
        controllingPlayer = true;
        Player.CanControl = false;
        Player.PlayerIronSteel.StopBurning();
        harmonySphere.GetComponent<Collider>().enabled = false;
        anim.SetTrigger("PlayerHasEntered");
        if (Unlocked)
            HUD.DisableHUD();

        //Player.PlayerIronSteel.StopBurning();
        Player.PlayerInstance.GetComponent<Rigidbody>().useGravity = false;
        CameraController.SetCinemachineCamera(vcam);
    }
    private void InsufficentSpikesMessage() {
        int spikesLeft = 3 - numSpikes;
        HUD.MessageOverlayCinematic.FadeIn(spikesLeft + " Spike" + (spikesLeft == 1 ? " remains" : "s remain"));
    }
    private void EndAnimation() {
        Player.PlayerInstance.SetFrameMaterial(GameManager.Material_MARLmetal_lit);
        //Player.PlayerInstance.GetComponentInChildren<MeshRenderer>().material = harmonySphere.GetComponent<Renderer>().material;
        foreach (Renderer renderer in symbolRenderers)
            renderer.material = GameManager.Material_MARLmetal_lit;

        // Open menus
        LevelCompletedScreen.OpenScreen(this);
    }
    private void EndAnimationInsufficient() {
        playerHasEntered = false;
        controllingPlayer = false;
        Player.CanControl = true;
        HUD.EnableHUD();
        Player.PlayerInstance.GetComponent<Rigidbody>().useGravity = SettingsMenu.settingsData.playerGravity == 1;
        CameraController.DisableCinemachineCamera(vcam);
        HUD.MessageOverlayCinematic.FadeOut();
        StartCoroutine(EnableColliderAfterDelay());
    }
    private IEnumerator EnableColliderAfterDelay() {
        yield return new WaitForSeconds(3);
        harmonySphere.GetComponent<Collider>().enabled = true;
    }
    private class HarmonySphere : MonoBehaviour {
        private void OnTriggerEnter(Collider other) {
            if (Player.IsPlayerTrigger(other)) {
                // Player has touched the target.
                GetComponentInParent<HarmonyTarget>().BeginAnimation();
            }
        }
    }

    public void ReleasePlayer() {
        controllingPlayer = false;
        anim.SetTrigger("PlayerExits");
        Player.CanControl = true;
        HUD.EnableHUD();
        Player.PlayerInstance.GetComponent<Rigidbody>().useGravity = SettingsMenu.settingsData.playerGravity == 1;
        CameraController.DisableCinemachineCamera(vcam);

        Destroy(harmonySphere.gameObject);
    }

    public void AddSpike() {
        if (numSpikes == 0) {
            spikeLeft.GetComponent<Renderer>().enabled = true;
        } else if (numSpikes == 1) {
            spikeRight.GetComponent<Renderer>().enabled = true;
        } else {
            spikeCenter.GetComponent<Renderer>().enabled = true;
        }
        numSpikes++;
        anim.SetInteger("SpikeCount", numSpikes);
    }

    public Vector3 GetNextSpikePosition() {
        if (numSpikes == 0) {
            return spikeLeft.TransformPoint(positionLeft);
        } else if (numSpikes == 1) {
            return spikeRight.TransformPoint(positionLeft);
        } else {
            return spikeCenter.TransformPoint(positionLeft);
        }
    }
    public Vector3 GetNextSpikeAngle() {
        if (numSpikes == 0) {
            return spikeLeft.TransformPoint(positionLeft * 2);
        } else if (numSpikes == 1) {
            return spikeRight.TransformPoint(positionLeft * 2);
        } else {
            return spikeCenter.TransformPoint(positionLeft * 2);
        }
    }
}
