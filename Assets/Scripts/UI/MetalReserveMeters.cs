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
    private MetalReserveElement pewter;
    
    private void Start() {
        iron = transform.Find("Iron").gameObject.AddComponent<MetalReserveElement>();
        iron.metalColor = new Color(0, .5f, 1);
        iron.reserve = Player.PlayerIronSteel.IronReserve;

        steel = transform.Find("Steel").gameObject.AddComponent<MetalReserveElement>();
        steel.metalColor = new Color(1, 0, 0);
        steel.reserve = Player.PlayerIronSteel.SteelReserve;

        pewter = transform.Find("Pewter").gameObject.AddComponent<MetalReserveElement>();
        pewter.metalColor = new Color(1, 0, 0);
        pewter.reserve = Player.PlayerPewter.PewterReserve;
    }

    private void LateUpdate() {
        UpdateReserve(iron);
        UpdateReserve(steel);
        UpdateReserve(pewter);
        UpdateReserveIronSteel(iron);
        UpdateReserveIronSteel(steel);
    }

    private void UpdateReserve(MetalReserveElement element) {
        if (element.reserve.Rate < 0)
            element.timeLastChanged = Time.time;

        element.massText.text = HUD.RoundStringToSigFigs((float)element.reserve.Mass, 3) + "g";
        element.rateText.text = HUD.RoundStringToSigFigs((float)element.reserve.Rate * 1000, 2) + "mg/s";
        element.fill.fillAmount = (float)element.reserve.Mass / maxMass;

        element.animator.SetBool("IsLow", element.fill.fillAmount < lowThreshold);
        element.animator.SetBool("IsVisible", Time.time - element.timeLastChanged < timeToFade);
    }

    private void UpdateReserveIronSteel(MetalReserveElement element) {
        // The -.001f is to account for floating-point error
        element.animator.SetBool("IsDraining", element.reserve.Rate < AllomanticIronSteel.gramsPerSecondPassiveBurn - .001f || element.reserve.Mass < criticalMassThreshold && element.reserve.Mass != 0);
    }

    public void Clear() {
        iron.Clear();
        steel.Clear();
        pewter.Clear();
    }

    // Called to flash the reserve meters on screen
    public void AlertIron() {
        iron.Alert();
    }
    public void AlertSteel() {
        steel.Alert();
    }

    private class MetalReserveElement : MonoBehaviour {

        public MetalReserve reserve;
        public float timeLastChanged = -100;
        public Animator animator;
        public Color metalColor;
        public Image fill;
        public Text massText;
        public Text rateText;

        private void Awake() {
            animator = GetComponent<Animator>();
            fill = transform.GetChild(1).GetComponentInChildren<Image>();
            massText = transform.GetChild(2).GetComponent<Text>();
            rateText = transform.GetChild(3).GetComponent<Text>();
        }

        public void Clear() {
            massText.text = "";
            rateText.text = "";
            animator.SetBool("IsVisible", false);
            animator.Play("MetalReserve_Invisible", animator.GetLayerIndex("Visibility"));
            timeLastChanged = -100;
        }

        public void Alert() {
            timeLastChanged = Time.time;
        }
    }
}
