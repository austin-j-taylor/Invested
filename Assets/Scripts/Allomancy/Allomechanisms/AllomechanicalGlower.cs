using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllomechanicalGlower : MonoBehaviour {

    private const int intensity = 2;
    // Metal/color indices
    private const int iron = 0;
    private const int steel = 1;
    
    private Color[] glowColors = {
            new Color(0, .35f, 1f),
            new Color(.7f, .025f, 0f)
    };

    [SerializeField]
    private Renderer[] irons;
    [SerializeField]
    private Renderer[] steels;

    private AllomanticIronSteel allomancer;

    private void Start() {
        allomancer = Player.PlayerIronSteel;
    }
    
    void LateUpdate()
    {
        if (allomancer.IsBurningIronSteel) {
            if (allomancer.IronPulling) {
                foreach (Renderer rend in irons) {
                    EnableEmission(rend.material, glowColors[iron], allomancer.IronBurnRateTarget);
                }
            } else {
                foreach (Renderer rend in irons) {
                    DisableEmission(rend.material);
                }
            }
            if (allomancer.SteelPushing) {
                foreach (Renderer rend in steels) {
                    EnableEmission(rend.material, glowColors[steel], allomancer.SteelBurnRateTarget);
                }
            } else {
                foreach (Renderer rend in steels) {
                    DisableEmission(rend.material);
                }
            }
        }
    }

    // Enables the emissions of the material specified by mat.
    private void EnableEmission(Material mat, Color glow, float rate) {
        mat.SetColor("_EmissionColor", glow * Mathf.LinearToGammaSpace(1 + intensity * rate));
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
