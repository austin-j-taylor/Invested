using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Makes the player's body appear transparent when the camera is looking at it/near it.
 */
public class PlayerTransparencyController : MonoBehaviour {

    private const float distanceThreshold = 2;
    private const float distanceThresholdInvisible = 0;//.5f;
    private const float lookAtTransparency = 0;
    //private const float lookAtTransparency = .15f;

    // Transparent materials
    private Renderer[] rends;
    [SerializeField]
    private Renderer transluscentPlayerCase = null;
    //[SerializeField]
    //private Renderer[] rendIron = null, rendSteel = null, rendPewter = null;
    //[SerializeField]
    //private Renderer rendGlass = null, rendFrame = null;

    private bool isOpaque, overrideHidden = false, isHidden = false;

    private float playerCaseMaxTransluscency;

    void Awake() {
        rends = GetComponentsInChildren<MeshRenderer>();
        playerCaseMaxTransluscency = transluscentPlayerCase.material.GetColor("_BaseColor").a;

        //playerCaseMaxTransluscency = rendGlass.material.GetFloat("_Opacity");
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
        //Color tempColor;
        if (isOpaque) {

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
        foreach (Renderer rend in rends) {
            if (rend == transluscentPlayerCase) {
                //rend.material.SetFloat("_Opacity", percent * playerCaseMaxTransluscency);
                Color temp = rend.material.GetColor("_BaseColor");
                temp.a = percent * playerCaseMaxTransluscency;
                rend.material.SetColor("_BaseColor", temp);
            } else {
                foreach (Material mat in rend.materials) {
                    mat.SetFloat("_Opacity", percent);
                }
            }
        }
        //foreach(Renderer rend in rendSteel) {
        //    rend.material.SetFloat("_Opacity", percent);
        //}
        //foreach (Renderer rend in rendPewter) {
        //    rend.material.SetFloat("_Opacity", percent);
        //}
        //rendGlass.material.SetFloat("_Opacity", percent);

        //rendFrame.material.SetFloat("_Opacity", percent);
    }

    // Set the rendering mode to Opaque
    private void SetAllOpaque() {
        if (!isOpaque) {
            foreach (Renderer rend in rends) {
                if (rend == transluscentPlayerCase) {
                    Color temp = rend.material.GetColor("_BaseColor");
                    temp.a = playerCaseMaxTransluscency;
                    rend.material.SetColor("_BaseColor", temp);
                } else {
                    foreach (Material mat in rend.materials) {
                        rend.material.SetFloat("_Opacity", 1);
                    }
                }
            }
            //foreach (Renderer rend in rendIron) {
            //    rend.material.SetFloat("_Opacity", 1);
            //}
            //foreach (Renderer rend in rendSteel) {
            //    rend.material.SetFloat("_Opacity", 1);
            //}
            //foreach (Renderer rend in rendPewter) {
            //    rend.material.SetFloat("_Opacity", 1);
            //}
            //rendGlass.material.SetFloat("_Opacity", 1);
            //rendFrame.material.SetFloat("_Opacity", 1);

            //Color tempColor;
            //foreach (Renderer rend in rends) {
            //    if (rend == transluscentPlayerCase) {
            //        transluscentPlayerCase.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            //        transluscentPlayerCase.material.DisableKeyword("_ALPHABLEND_ON");
            //        transluscentPlayerCase.material.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");

            //        tempColor = transluscentPlayerCase.material.GetColor("_BaseColor");
            //        tempColor.a = playerCaseMaxTransluscency;
            //        transluscentPlayerCase.material.GetColor("_BaseColor") = tempColor;
            //    } else {
            //        foreach (Material mat in rend.materials) {
            //            if (mat.mainTexture) {
            //                tempColor = mat.GetColor("_Color");
            //                //tempColor = mat.GetColor("_BaseColor");
            //                tempColor.a = 1;
            //                mat.SetColor("_BaseColor", tempColor);
            //                //mat.GetColor("_BaseColor") = tempColor;
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
            foreach (Renderer rend in rends) {
                if (rend) {
                    rend.enabled = !hidden;
                }
            }
            //foreach (Renderer rend in rendIron)
            //    if (rend)
            //        rend.enabled = !hidden;
            //foreach (Renderer rend in rendSteel)
            //    if (rend)
            //        rend.enabled = !hidden;
            //foreach (Renderer rend in rendPewter)
            //    if (rend)
            //        rend.enabled = !hidden;
            //if (rendGlass)
            //    rendGlass.enabled = !hidden;
            //if (rendFrame)
            //    rendFrame.enabled = !hidden;
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
                //foreach (Renderer rend in rendIron)
                //    if (rend)
                //        rend.enabled = !hidden;
                //foreach (Renderer rend in rendSteel)
                //    if (rend)
                //        rend.enabled = !hidden;
                //foreach (Renderer rend in rendPewter)
                //    if (rend)
                //        rend.enabled = !hidden;
                //if (rendGlass)
                //    rendGlass.enabled = !hidden;
                //if (rendFrame)
                //    rendFrame.enabled = !hidden;
            }
        }
    }
}
