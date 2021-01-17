using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Makes the player's body appear transparent when the camera is looking at it/near it.
 */
public class PrimaTransparencyController : MonoBehaviour {

    private const float distanceThreshold = 2;
    private const float distanceThresholdInvisible = 0;//.5f;
    private const float lookAtTransparency = .15f;

    private Renderer[] rends;
    private bool isOpaque, overrideHidden = false, isHidden = false;

    private Renderer transluscentPlayerCase;
    private float playerCaseMaxTransluscency;

    void Awake() {
        rends = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < rends.Length; i++) {
            if (rends[i].material.color.a != 1) {
                transluscentPlayerCase = rends[i];
                playerCaseMaxTransluscency = transluscentPlayerCase.material.color.a;
            }
        }
        SetAllOpaque();
    }

    void LateUpdate() {
        // Player is always visible if the Camera is doing something cinematic
        if (!overrideHidden && GameManager.CameraState == GameManager.GameCameraState.Standard && Player.CanControl) {
            float distance = (CameraController.ActiveCamera.transform.position - Player.PlayerInstance.transform.position).magnitude;
            // If the camera is SUPER close to the body, make it invisible
            if (distance < distanceThresholdInvisible) {
                SetHidden(true);
            } else {
                SetHidden(false);
                // If the camera is directly looking at the player, set the transparency to a constant amount
                if (Physics.Raycast(CameraController.ActiveCamera.transform.position, CameraController.ActiveCamera.transform.forward, out RaycastHit hit, distance, 1 << LayerMask.NameToLayer("Player"))) {
                    // If reticle is on player, immediately fade to transparent
                    SetAllFade(lookAtTransparency);
                } else {
                    // If camera is physically near the player, fade slowly to transparent
                    float realThreshold = distanceThreshold * SettingsMenu.settingsGameplay.cameraDistance / 5;
                    if (SettingsMenu.settingsGameplay.cameraFirstPerson == 0 && distance < realThreshold) {
                        float percent = ((distance * distance) / (realThreshold * realThreshold));
                        if (percent >= 0)
                            SetAllFade(percent);
                        else
                            SetAllOpaque();
                    } else {
                        SetAllOpaque();
                    }
                }
            }
        } else {
            SetAllOpaque();
        }

    }

    public void Clear() {
        //if(SettingsMenu.settingsData.cameraFirstPerson == 0) {
        //    SetOverrideHidden(false);
        //} else {
        //    SetOverrideHidden(true);
        //}
        //LateUpdate();
    }

    // Set the rendering mode to Fade, and set the transparency to percent
    private void SetAllFade(float percent) {
        Color tempColor;
        if (isOpaque) {
            foreach (Renderer rend in rends) {
                foreach (Material mat in rend.materials) {
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
                    mat.renderQueue = 3000;
                }
            }
            isOpaque = false;
        }
        foreach (Renderer rend in rends) {
            if (rend == transluscentPlayerCase) {
                tempColor = rend.material.color;
                tempColor.a = percent * playerCaseMaxTransluscency;
                rend.material.color = tempColor;
            } else {
                foreach (Material mat in rend.materials) {
                    tempColor = mat.color;
                    tempColor.a = percent;
                    mat.color = tempColor;
                }
            }
        }
    }

    // Set the rendering mode to Opaque
    private void SetAllOpaque() {
        if (!isOpaque) {
            Color tempColor;
            foreach (Renderer rend in rends) {
                if (rend == transluscentPlayerCase) {
                    transluscentPlayerCase.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    transluscentPlayerCase.material.DisableKeyword("_ALPHABLEND_ON");
                    transluscentPlayerCase.material.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");

                    tempColor = transluscentPlayerCase.material.color;
                    tempColor.a = playerCaseMaxTransluscency;
                    transluscentPlayerCase.material.color = tempColor;
                } else {
                    foreach (Material mat in rend.materials) {
                        if (mat.mainTexture) {
                            tempColor = mat.color;
                            tempColor.a = 1;
                            mat.color = tempColor;
                        } else {
                            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                            mat.SetInt("_ZWrite", 1);
                            mat.DisableKeyword("_ALPHABLEND_ON");
                            mat.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
                            mat.renderQueue = -1;
                        }
                    }
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
