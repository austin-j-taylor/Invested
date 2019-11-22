using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Makes the player's body appear transparent when the camera is looking at it/near it.
 */
public class PlayerTransparencyController : MonoBehaviour {

    private const float distanceThreshold = 2;
    private const float distanceThresholdInvisible = .5f;
    private const float lookAtTransparency = .15f;

    private Renderer[] rends;
    private bool isOpaque, overrideHidden = false, isHidden = false;


    void Awake() {
        rends = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < rends.Length; i++) {
            if (rends[i].material.color.a != 1) {
                rends[i] = null;
            }
        }
        SetAllOpaque();
    }

    void LateUpdate() {
        if (!overrideHidden) {
            float distance = (CameraController.ActiveCamera.transform.position - Player.PlayerInstance.transform.position).magnitude;
            // If the camera is SUPER close to the body, make it invisible
            if (distance < distanceThresholdInvisible) {
                SetHidden(true);
            } else {
                SetHidden(false);
                float percent = -1;
                // If camera is physically near the player, fade slowly to transparent
                float realThreshold = distanceThreshold * SettingsMenu.settingsData.cameraDistance / 10;
                if (SettingsMenu.settingsData.cameraFirstPerson == 0 && distance < realThreshold) {
                    percent = ((distance * distance) / (realThreshold * realThreshold));
                }
                // If the camera is directly looking at the player, set the transparency to a constant amount
                if ((percent == -1 || percent > lookAtTransparency) && Physics.Raycast(CameraController.ActiveCamera.transform.position, CameraController.ActiveCamera.transform.forward, out RaycastHit hit, distance, 1 << LayerMask.NameToLayer("Player"))) {
                    // If reticle is on player, immediately fade to transparent
                    percent = (lookAtTransparency);
                }
                // Assign fade/opaque Rendering Mode
                if (percent >= 0)
                    SetAllFade(percent);
                else // Do not fade camera at all
                    SetAllOpaque();
            }
        } else {
            SetAllOpaque();
        }

    }

    // Set the rendering mode to Fade, and set the transparency to percent
    private void SetAllFade(float percent) {
        foreach (Renderer rend in rends) {
            if (rend) {
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
        }
        isOpaque = false;
    }

    // Set the rendering mode to Opaque
    private void SetAllOpaque() {
        if (!isOpaque) {
            foreach (Renderer rend in rends) {
                if (rend) {
                    rend.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    rend.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    rend.material.SetInt("_ZWrite", 1);
                    rend.material.DisableKeyword("_ALPHABLEND_ON");
                    rend.material.renderQueue = -1;
                }
            }
            isOpaque = true;
        }
    }

    public void SetOverrideHidden(bool hidden) {
        if (hidden != overrideHidden) {
            overrideHidden = hidden;
            isHidden = hidden;
            foreach (Renderer rend in rends) {
                if (rend) {
                    rend.enabled = !hidden;
                }
            }
        }
    }

    private void SetHidden(bool hidden) {
        if (!overrideHidden) { // do nothing if we should always be hidden
            if (hidden != isHidden) {
                isHidden = hidden;
                foreach (Renderer rend in rends) {
                    if (rend) {
                        rend.enabled = !hidden;
                    }
                }
            }
        }
    }
}
