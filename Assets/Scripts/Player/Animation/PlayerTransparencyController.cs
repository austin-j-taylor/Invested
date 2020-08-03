using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Makes the player's body appear transparent when the camera is looking at it/near it.
 */
public class PlayerTransparencyController : MonoBehaviour {

    private const float distanceThreshold = 2;
    private const float distanceThresholdInvisible = 0;//.5f;
    private const float lookAtTransparency = .15f;

    // Transparent materials
    [SerializeField]
    private Material fadeIron = null, fadeSteel = null, fadePewter = null, fadeGlass = null, fadeFrame = null;
    [SerializeField]
    private Renderer[] rendIron = null, rendSteel = null, rendPewter = null;
    [SerializeField]
    private Renderer rendGlass = null, rendFrame = null;
    private Material opaqIron = null, opaqSteel = null, opaqPewter = null, opaqGlass = null, opaqFrame = null;

    private bool isOpaque, overrideHidden = false, isHidden = false;

    private float playerCaseMaxTransluscency;

    void Awake() {
        opaqIron = rendIron[0].material;
        opaqSteel = rendSteel[0].material;
        opaqPewter = rendPewter[0].material;
        opaqGlass = rendGlass.material;
        opaqFrame = rendFrame.material;

        playerCaseMaxTransluscency = opaqFrame.color.a;
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
            foreach (Renderer rend in rendIron) {
                rend.material = fadeIron;
                tempColor = rend.material.GetColor("_Color");
                tempColor.a = percent;
                rend.material.SetColor("_BaseColor", tempColor);
            }
            foreach(Renderer rend in rendSteel) {
                rend.material = fadeSteel;
                tempColor = rend.material.GetColor("_Color");
                tempColor.a = percent;
                rend.material.SetColor("_BaseColor", tempColor);
            }
            foreach (Renderer rend in rendPewter) {
                rend.material = fadePewter;
                tempColor = rend.material.GetColor("_Color");
                tempColor.a = percent;
                rend.material.SetColor("_BaseColor", tempColor);
            }
            rendGlass.material = fadeGlass;
            tempColor = rendGlass.material.GetColor("_Color");
            tempColor.a = percent;
            rendGlass.material.SetColor("_BaseColor", tempColor);

            rendFrame.material = fadeFrame;
            tempColor = rendFrame.material.GetColor("_Color");
            tempColor.a = percent;
            rendFrame.material.SetColor("_BaseColor", tempColor);
            //foreach (Renderer rend in rends) {
            //    foreach (Material mat in rend.materials) {
            //        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            //        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            //        mat.EnableKeyword("_ALPHABLEND_ON");
            //        mat.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            //        mat.renderQueue = 3000;
            //    }
            //}
            isOpaque = false;
        }
        //foreach (Renderer rend in rends) {
        //    if (rend == transluscentPlayerCase) {
        //        tempColor = rend.material.GetColor("_Color");
        //        tempColor.a = percent * playerCaseMaxTransluscency;
        //        rend.material.SetColor("_BaseColor", tempColor);
        //    } else {
        //        foreach (Material mat in rend.materials) {
        //            tempColor = mat.GetColor("_Color");
        //            tempColor.a = percent;
        //            mat.SetColor("_BaseColor", tempColor);
        //        }
        //    }
        //}
    }

    // Set the rendering mode to Opaque
    private void SetAllOpaque() {
        if (!isOpaque) {
            foreach (Renderer rend in rendIron) {
                rend.material = opaqIron;
            }
            foreach (Renderer rend in rendSteel) {
                rend.material = opaqSteel;
            }
            foreach (Renderer rend in rendPewter) {
                rend.material = opaqPewter;
            }
            rendGlass.material = opaqGlass;

            rendFrame.material = opaqFrame;
            //Color tempColor;
            //foreach (Renderer rend in rends) {
            //    if (rend == transluscentPlayerCase) {
            //        transluscentPlayerCase.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            //        transluscentPlayerCase.material.DisableKeyword("_ALPHABLEND_ON");
            //        transluscentPlayerCase.material.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");

            //        tempColor = transluscentPlayerCase.material.color;
            //        tempColor.a = playerCaseMaxTransluscency;
            //        transluscentPlayerCase.material.color = tempColor;
            //    } else {
            //        foreach (Material mat in rend.materials) {
            //            if (mat.mainTexture) {
            //                tempColor = mat.GetColor("_Color");
            //                //tempColor = mat.color;
            //                tempColor.a = 1;
            //                mat.SetColor("_BaseColor", tempColor);
            //                //mat.color = tempColor;
            //            } else {
            //                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            //                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            //                mat.SetInt("_ZWrite", 1);
            //                mat.DisableKeyword("_ALPHABLEND_ON");
            //                mat.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
            //                mat.renderQueue = -1;
            //            }
            //        }
            //    }
            //}
            isOpaque = true;
        }
    }

    public void SetOverrideHidden(bool hidden) {
        if (hidden != overrideHidden) {
            overrideHidden = hidden;
            isHidden = hidden;
            //foreach (Renderer rend in rends) {
            //    if (rend) {
            //        rend.enabled = !hidden;
            //    }
            //}
            foreach (Renderer rend in rendIron)
                if (rend)
                    rend.enabled = !hidden;
            foreach (Renderer rend in rendSteel)
                if (rend)
                    rend.enabled = !hidden;
            foreach (Renderer rend in rendPewter)
                if (rend)
                    rend.enabled = !hidden;
            if (rendGlass)
                rendGlass.enabled = !hidden;
            if (rendFrame)
                rendFrame.enabled = !hidden;
        }
    }

    private void SetHidden(bool hidden) {
        if (!overrideHidden) { // do nothing if we should always be hidden
            if (hidden != isHidden) {
                isHidden = hidden;
                //foreach (Renderer rend in rends) {
                //    if (rend) {
                //        rend.enabled = !hidden;
                //    }
                //}
                foreach (Renderer rend in rendIron)
                    if (rend)
                        rend.enabled = !hidden;
                foreach (Renderer rend in rendSteel)
                    if (rend)
                        rend.enabled = !hidden;
                foreach (Renderer rend in rendPewter)
                    if (rend)
                        rend.enabled = !hidden;
                if (rendGlass)
                    rendGlass.enabled = !hidden;
                if (rendFrame)
                    rendFrame.enabled = !hidden;
            }
        }
    }
}
