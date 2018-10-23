using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MetalReserveMeters : MonoBehaviour {

    private const float maxMass = 100;

    private MetalReserveElement iron;
    private MetalReserveElement steel;

    private void Start() {
        iron = transform.GetChild(0).gameObject.AddComponent<MetalReserveElement>();
        iron.metalColor = new Color(0, .5f, 1);
        iron.reserve = Player.PlayerIronSteel.IronReserve;

        steel = transform.GetChild(1).gameObject.AddComponent<MetalReserveElement>();
        steel.metalColor = new Color(1, 0, 0);
        steel.reserve = Player.PlayerIronSteel.SteelReserve;
    }

    private void Update() {
        iron.massText.text = HUD.RoundStringToSigFigs((float)iron.reserve.Mass, 3) + "g";
        iron.rateText.text = HUD.RoundStringToSigFigs((float)iron.reserve.Rate * 1000, 2) + "mg/s";
        iron.fill.fillAmount = (float)iron.reserve.Mass / maxMass;

        steel.massText.text = HUD.RoundStringToSigFigs((float)steel.reserve.Mass, 3) + "g";
        steel.rateText.text = HUD.RoundStringToSigFigs((float)steel.reserve.Rate * 1000, 2) + "mg/s";
        steel.fill.fillAmount = (float)steel.reserve.Mass / maxMass;

    }

    public void Clear() {
        iron.Clear();
        steel.Clear();
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
