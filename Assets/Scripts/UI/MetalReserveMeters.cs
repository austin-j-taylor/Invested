using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the HUD elements showing the amount of metal in the player's reserves
/// </summary>
public class MetalReserveMeters : MonoBehaviour {

    private const float lowThreshold = .25f; // When reserve is < 20%
    private const float criticalMassThreshold = 0.1f; // When reserve is < 10%
    private const float timeToFade = 1;

    //private MetalReserveElement iron;
    //private MetalReserveElement steel;
    private MetalReserveElement pewter;

    private void Awake() {
        //iron = transform.Find("Iron").gameObject.AddComponent<MetalReserveElement>();
        //iron.metalColor = new Color(0, .5f, 1);
        //iron.reserve = Player.PlayerIronSteel.IronReserve;

        //steel = transform.Find("Steel").gameObject.AddComponent<MetalReserveElement>();
        //steel.metalColor = new Color(1, 0, 0);
        //steel.reserve = Player.PlayerIronSteel.SteelReserve;

        pewter = transform.Find("Pewter").gameObject.AddComponent<MetalReserveElement>();
        pewter.metalColor = new Color(1, 0, 0);
        pewter.reserve = Player.PlayerPewter.PewterReserve;
    }

    private void LateUpdate() {
        if (!PauseMenu.IsPaused) {
            //UpdateReserve(iron);
            //UpdateReserve(steel);
            UpdateReserve(pewter);
        }
    }

    private void UpdateReserve(MetalReserveElement element) {
        if (element.reserve.IsChanging)
            element.timeLastChanged = Time.time;

        //element.massText.text = HUD.RoundStringToSigFigs((float)element.reserve.Mass, 3) + "g";
        //double threshold = .100 * Time.fixedDeltaTime;
        //if (element.reserve.Rate < threshold && element.reserve.Rate > -threshold) {
        //    if(element.reserve.IsChanging) {
        //        element.rateText.text = "<100mg/s";
        //    } else {
        //        element.rateText.text = "";
        //    }
        //} else {
        //    element.rateText.text = HUD.RoundStringToSigFigs((float)element.reserve.Rate * 1000, 2) + "mg/s";
        //}
        element.fill.fillAmount = (float)(element.reserve.Mass / element.reserve.Capacity);

        element.animator.SetBool("IsLow", element.fill.fillAmount < lowThreshold * element.reserve.Capacity);
        element.animator.SetBool("IsVisible", Time.time - element.timeLastChanged < timeToFade);

        element.animator.SetBool("IsFlashing", element.reserve.IsBurnedOut);
    }

    public void Clear() {
        //iron.Clear();
        //steel.Clear();
        pewter.Clear();
    }

    // Called to flash the reserve meters on screen
    public void AlertIron() {
        //iron.Alert();
    }
    public void AlertSteel() {
        //steel.Alert();
    }
    public void AlertPewter() {
        pewter.Alert();
    }

    /// <summary>
    /// Controls a single metal reserve's appearance on the HUD, such as iron or steel
    /// </summary>
    private class MetalReserveElement : MonoBehaviour {

        public MetalReserve reserve;
        public float timeLastChanged = -100;
        public Animator animator;
        public Color metalColor;
        public Image fill;
        //public Text massText;
        //public Text rateText;

        private void Awake() {
            animator = GetComponent<Animator>();
            fill = transform.GetChild(1).GetComponentInChildren<Image>();
            //massText = transform.GetChild(2).GetComponent<Text>();
            //rateText = transform.GetChild(3).GetComponent<Text>();
        }

        public void Clear() {
            //massText.text = "";
            //rateText.text = "";
            animator.SetBool("IsVisible", false);
            animator.Play("MetalReserve_Invisible", animator.GetLayerIndex("Visibility"));
            timeLastChanged = -100;
        }

        public void Alert() {
            timeLastChanged = Time.time;
        }
    }
}
