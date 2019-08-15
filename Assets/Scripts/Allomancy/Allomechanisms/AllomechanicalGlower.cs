using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllomechanicalGlower : MonoBehaviour {

    private const int intensity = 2;
    // Metal/color indices
    private const int iron = 0;
    private const int steel = 1;
    private const int pewter = 2;
    
    private readonly Color[] glowColors = {
            new Color(0, .35f, 1f),
            new Color(.7f, .025f, 0.05f),
            new Color(.75f, .25f, 0f)
    };
    
    private Renderer[] irons;
    private Renderer[] steels;
    private Renderer[] pewters;

    private AllomanticIronSteel allomancer;
    private AllomanticPewter allomancerPewter;

    private void Start() {
        allomancer = Player.PlayerIronSteel;
        allomancerPewter = Player.PlayerPewter;
        irons = transform.Find("Irons").GetComponentsInChildren<Renderer>();
        steels = transform.Find("Steels").GetComponentsInChildren<Renderer>();
        pewters = transform.Find("Pewters").GetComponentsInChildren<Renderer>();
    }
    
    void LateUpdate() {
        if (allomancer.IsBurning) {
            if (allomancer.IronPulling) {
                foreach (Renderer rend in irons) {
                    EnableEmission(rend.material, glowColors[iron], 1 + 2 * allomancer.IronBurnPercentageTarget);
                }
            } else {
                foreach (Renderer rend in irons) {
                    DisableEmission(rend.material);
                }
            }
            if (allomancer.SteelPushing) {
                foreach (Renderer rend in steels) {
                    EnableEmission(rend.material, glowColors[steel], 1 + 2 * allomancer.SteelBurnPercentageTarget);
                }
            } else {
                foreach (Renderer rend in steels) {
                    DisableEmission(rend.material);
                }
            }
        }
        if (allomancerPewter.IsBurning) {
            foreach (Renderer rend in pewters) {
                EnableEmission(rend.material, glowColors[pewter], -2 * (float)allomancerPewter.PewterReserve.Rate);
            }
        } else {
            foreach (Renderer rend in pewters) {
                DisableEmission(rend.material);
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
