using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MetalReserveMeters : MonoBehaviour {

    private const float maxMass = 100;
    private const float lowThreshold = .15f;
    private const float criticalMassThreshold = 1f; // When reserve is < 1g, always flash red
    private const float timeToFade = 5;

    private MetalReserveElement iron;
    private MetalReserveElement steel;
    private Animator ironAnimator;
    private Animator steelAnimator;

    private float timeLastChangedIron = -1000;
    private float timeLastChangedSteel = -1000;

    private void Start() {
        iron = transform.GetChild(0).gameObject.AddComponent<MetalReserveElement>();
        ironAnimator = iron.GetComponent<Animator>();
        iron.metalColor = new Color(0, .5f, 1);
        iron.reserve = Player.PlayerIronSteel.IronReserve;

        steel = transform.GetChild(1).gameObject.AddComponent<MetalReserveElement>();
        steelAnimator = steel.GetComponent<Animator>();
        steel.metalColor = new Color(1, 0, 0);
        steel.reserve = Player.PlayerIronSteel.SteelReserve;
    }

    private void Update() {
        if (iron.reserve.Rate < 0)
            timeLastChangedIron = Time.time;
        if(steel.reserve.Rate < 0)
            timeLastChangedSteel = Time.time;

        iron.massText.text = HUD.RoundStringToSigFigs((float)iron.reserve.Mass, 3) + "g";
        iron.rateText.text = HUD.RoundStringToSigFigs((float)iron.reserve.Rate * 1000, 2) + "mg/s";
        iron.fill.fillAmount = (float)iron.reserve.Mass / maxMass;

        steel.massText.text = HUD.RoundStringToSigFigs((float)steel.reserve.Mass, 3) + "g";
        steel.rateText.text = HUD.RoundStringToSigFigs((float)steel.reserve.Rate * 1000, 2) + "mg/s";
        steel.fill.fillAmount = (float)steel.reserve.Mass / maxMass;

        ironAnimator.SetBool("IsLow", iron.fill.fillAmount < lowThreshold);
        // The -.001f is to account for floating-point error
        ironAnimator.SetBool("IsDraining", iron.reserve.Rate < AllomanticIronSteel.gramsPerSecondPassiveBurn - .001f || iron.reserve.Mass < criticalMassThreshold && iron.reserve.Mass != 0);
        ironAnimator.SetBool("IsVisible", Time.time - timeLastChangedIron < timeToFade);
        steelAnimator.SetBool("IsLow", steel.fill.fillAmount < lowThreshold);
        steelAnimator.SetBool("IsDraining", steel.reserve.Rate < AllomanticIronSteel.gramsPerSecondPassiveBurn - .001f || steel.reserve.Mass < criticalMassThreshold && steel.reserve.Mass != 0);
        steelAnimator.SetBool("IsVisible", Time.time - timeLastChangedSteel < timeToFade);
    }

    public void Clear() {
        iron.Clear();
        steel.Clear();
        ironAnimator.SetBool("IsVisible", false);
        ironAnimator.Play("MetalReserve_Invisible", ironAnimator.GetLayerIndex("Visibility"));
        steelAnimator.SetBool("IsVisible", false);
        steelAnimator.Play("MetalReserve_Invisible", steelAnimator.GetLayerIndex("Visibility"));
        timeLastChangedIron = -100;
        timeLastChangedSteel = -100;
    }

    // Called to flash the reserve meters on screen
    public void AlertIron() {
        timeLastChangedIron = Time.time;
    }
    public void AlertSteel() {
        timeLastChangedSteel = Time.time;
    }

    private class MetalReserveElement : MonoBehaviour {

        public MetalReserve reserve;
        public Color metalColor;
        public Image fill;
        public Text massText;
        public Text rateText;

        private void Awake() {
            fill = transform.GetChild(1).GetComponentInChildren<Image>();
            massText = transform.GetChild(2).GetComponent<Text>();
            rateText = transform.GetChild(3).GetComponent<Text>();
        }

        public void Clear() {
            massText.text = "";
            rateText.text = "";
        }
    }
}
