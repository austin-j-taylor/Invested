using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllomechanicalGlower : MonoBehaviour {

    private const int intensity = 2;
    // Metal/color indices

    public readonly static Color ColorIron = new Color(0, .35f, 1f);
    public readonly static Color ColorIronTransparent = new Color(0, .35f, 1f, 0.01960784f);
    public readonly static Color ColorSteel = new Color(.7f, .025f, 0.05f);
    public readonly static Color ColorSteelTransparent = new Color(.7f, .025f, 0.05f, 0.01960784f);
    public readonly static Color ColorPewter = new Color(.6f, .7f, 1f);
    //public readonly static Color ColorPewter = new Color(.75f, .25f, 0f);

    private Renderer[] irons;
    private Renderer[] steels;
    private Renderer[] pewters;
    private Renderer[] zincs;

    private void Awake() {

        irons = transform.Find("Irons").GetComponentsInChildren<Renderer>();
        steels = transform.Find("Steels").GetComponentsInChildren<Renderer>();
        Renderer[] pewterSymbols = transform.Find("Pewters").GetComponentsInChildren<Renderer>();
        Transform wheels = transform.Find("Wheels");
        pewters = new Renderer[pewterSymbols.Length + wheels.childCount];
        for (int i = 0; i < pewterSymbols.Length; i++) {
            pewters[i] = pewterSymbols[i];
        }
        for (int i = pewterSymbols.Length; i < pewters.Length; i++) {
            pewters[i] = wheels.GetChild(i - pewterSymbols.Length).GetComponent<Renderer>();
        }
        zincs = transform.Find("ZincPeripheral").GetComponentsInChildren<Renderer>();

    }

    void LateUpdate() {
        if (!PauseMenu.IsPaused) {
            if (Player.PlayerIronSteel.IsBurning) {
                if (Player.PlayerIronSteel.IronPulling) {
                    foreach (Renderer rend in irons) {
                        EnableEmission(rend.material, ColorIron, 1 + 2 * Player.PlayerIronSteel.IronBurnPercentageTarget);
                    }
                } else {
                    foreach (Renderer rend in irons) {
                        DisableEmission(rend.material);
                    }
                }
                if (Player.PlayerIronSteel.SteelPushing) {
                    foreach (Renderer rend in steels) {
                        EnableEmission(rend.material, ColorSteel, 1 + 2 * Player.PlayerIronSteel.SteelBurnPercentageTarget);
                    }
                } else {
                    foreach (Renderer rend in steels) {
                        DisableEmission(rend.material);
                    }
                }
            }
            if (Player.PlayerPewter.IsBurning) {
                foreach (Renderer rend in pewters) {
                    EnableEmission(rend.material, ColorPewter, 1 + -4 * (float)Player.PlayerPewter.PewterReserve.Rate);
                }
            } else {
                foreach (Renderer rend in pewters) {
                    DisableEmission(rend.material);
                }
            }
            if(Player.PlayerZinc.InZincTime) {
                foreach (Renderer rend in zincs) {
                    EnableEmission(rend.material, ZincMeterController.ColorZinc, 1 + 2 * Player.PlayerZinc.Intensity);
                }
            } else {
                foreach (Renderer rend in zincs) {
                    DisableEmission(rend.material);
                }
            }
        }
    }

    // Enables the emissions of the material specified by mat.
    private void EnableEmission(Material mat, Color glow, float rate) {
        mat.SetColor("_EmissionColor", glow * Mathf.LinearToGammaSpace(intensity * rate));
        mat.EnableKeyword("_EMISSION");
    }

    private void DisableEmission(Material mat) {
        mat.DisableKeyword("_EMISSION");
    }

    public void RemoveAllEmissions() {
        foreach (Renderer rend in irons) {
            DisableEmission(rend.material);
        }
        foreach (Renderer rend in steels) {
            DisableEmission(rend.material);
        }
    }
}
