using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the glowing elements of Kog.
/// </summary>
public class KogAnimationGlower : MonoBehaviour {

    private const int intensity = 2;
    // Metal/color indices

    public readonly static Color ColorIron = new Color(0, .35f, 1f);
    public readonly static Color ColorIronTransparent = new Color(0, .35f, 1f, 0.01960784f);
    public readonly static Color ColorSteel = new Color(.7f, .025f, 0.05f);
    public readonly static Color ColorSteelTransparent = new Color(.7f, .025f, 0.05f, 0.01960784f);
    [SerializeField]
    private Renderer[] irons = null;
    [SerializeField]
    private Renderer[] steels = null;

    private void Awake() {

        irons = transform.Find("Irons").GetComponentsInChildren<Renderer>();
        steels = transform.Find("Steels").GetComponentsInChildren<Renderer>();
    }

    void LateUpdate() {
        if (!GameManager.MenusController.pauseMenu.IsOpen) {
            if (Prima.PrimaInstance.ActorIronSteel.IronPulling) {
                foreach (Renderer rend in irons) {
                    EnableEmission(rend.material, ColorIron, 1 + 2 * Prima.PrimaInstance.ActorIronSteel.IronBurnPercentageTarget);
                }
            } else {
                foreach (Renderer rend in irons) {
                    DisableEmission(rend.material);
                }
            }
            if (Prima.PrimaInstance.ActorIronSteel.SteelPushing) {
                foreach (Renderer rend in steels) {
                    EnableEmission(rend.material, ColorSteel, 1 + 2 * Prima.PrimaInstance.ActorIronSteel.SteelBurnPercentageTarget);
                }
            } else {
                foreach (Renderer rend in steels) {
                    DisableEmission(rend.material);
                }
            }
        }
    }

    public void Clear() {
        foreach (Renderer rend in irons) {
            DisableEmission(rend.material);
        }
        foreach (Renderer rend in steels) {
            DisableEmission(rend.material);
        }
    }

    //public void SetOverrideGlows(bool iron, bool steel, bool pewter, bool zinc) {
    //    isOverridden = true;
    //    if (iron)
    //        foreach (Renderer rend in irons)
    //            EnableEmission(rend.material, ColorIron, 3);
    //    else
    //        foreach (Renderer rend in irons)
    //            DisableEmission(rend.material);
    //    if (steel)
    //        foreach (Renderer rend in steels)
    //            EnableEmission(rend.material, ColorSteel, 3);
    //    else
    //        foreach (Renderer rend in steels)
    //            DisableEmission(rend.material);
    //    if (pewter)
    //        foreach (Renderer rend in pewters)
    //            EnableEmission(rend.material, ColorPewter, 3);
    //    else
    //        foreach (Renderer rend in pewters)
    //            DisableEmission(rend.material);
    //    if (zinc)
    //        foreach (Renderer rend in zincs)
    //            EnableEmission(rend.material, ZincMeterController.ColorZinc, 3);
    //    else
    //        foreach (Renderer rend in zincs)
    //            DisableEmission(rend.material);
    //}

    // Enables the emissions of the material specified by mat.
    private void EnableEmission(Material mat, Color glow, float rate) {
        mat.SetColor("_EmissionColor", glow * Mathf.LinearToGammaSpace(intensity * rate));
        mat.EnableKeyword("_EMISSION");
    }

    private void DisableEmission(Material mat) {
        mat.DisableKeyword("_EMISSION");
    }
}
