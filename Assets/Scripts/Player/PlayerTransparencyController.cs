using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Makes the player's body appear transparent when the camera is looking at it/near it.
 */
public class PlayerTransparencyController : MonoBehaviour {

    private const int threshold = 1;
    private const float lookAtTransparency = .15f;

    private Renderer[] rends;
    private bool isOpaque;
    
    void Start() {
        rends = GetComponentsInChildren<MeshRenderer>();
        SetAllOpaque();
    }
    
    void LateUpdate() {
        if (Player.CanControlMovement) {
            float distance = (CameraController.ActiveCamera.transform.position - Player.PlayerInstance.transform.position).magnitude;
            float percent = 0;
            // If camera is physically near the player, fade slowly to transparent
            if (SettingsMenu.settingsData.cameraFirstPerson == 0 && distance < threshold) {
                percent = ((distance * distance) / (threshold * threshold));
            }
            // If the camera is directly looking at the player, set the transparency to a constant amount
            if (Physics.Raycast(CameraController.ActiveCamera.transform.position, CameraController.ActiveCamera.transform.forward, out RaycastHit hit, distance, 1 << LayerMask.NameToLayer("Player"))) {
                // If reticle is on player, immediately fade to transparent
                percent = (lookAtTransparency);
            }
            // Assign fade/opaque Rendering Mode
            if (percent > 0)
                SetAllFade(percent);
            else // Do not fade camera at all
                SetAllOpaque();
        } else {
            SetAllOpaque();
        }

    }

    // Set the rendering mode to Fade, and set the transparency to percent
    private void SetAllFade(float percent) {
        foreach (Renderer rend in rends) {
            if (isOpaque) {
                rend.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                rend.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                rend.material.EnableKeyword("_ALPHABLEND_ON");
                rend.material.renderQueue = 3000;
            }

            Color tempColor = rend.material.color;
            tempColor.a = percent;
            rend.material.color = tempColor;
        }
        isOpaque = false;
    }

    // Set the rendering mode to Opaque
    private void SetAllOpaque() {
        if (!isOpaque) {
            foreach (Renderer rend in rends) {
                rend.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                rend.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                rend.material.SetInt("_ZWrite", 1);
                rend.material.DisableKeyword("_ALPHABLEND_ON");
                rend.material.renderQueue = -1;
            }
            isOpaque = true;
        }
    }
}
