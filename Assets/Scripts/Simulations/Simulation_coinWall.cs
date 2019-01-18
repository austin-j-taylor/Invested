using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Simulation_coinWall : MonoBehaviour {

    //private float timeToReset;
    private NonPlayerPushPullController allomancer;
    private Magnetic coin;
    
    private Text[] texts;

    // Use this for initialization
    void Start() {
        allomancer = GetComponentInChildren<NonPlayerPushPullController>();
        coin = GetComponentInChildren<Magnetic>();

        allomancer.AddPushTarget(coin);
        allomancer.SteelPushing = true;
        allomancer.SteelBurnRateTarget = 1;
        allomancer.PushTargets.MaxRange = 50;
        texts = GameObject.FindGameObjectWithTag("Canvas").transform.Find("Simulations").Find("coinWall").GetComponentsInChildren<Text>();


        if (SettingsMenu.settingsData.anchoredBoost == 1) {
            texts[texts.Length - 2].text = "Allomantic Normal Force";
            Time.timeScale = .04f;
            Time.fixedDeltaTime = Time.timeScale * 1 / 60f;
        } else {
            texts[texts.Length - 1].text = "Exponential w/ Velocity factor";
            Time.timeScale = .2f;
            Time.fixedDeltaTime = Time.timeScale * 1 / 60f;
        }

        texts[texts.Length - 4].text = "Time scale: " + Time.timeScale;
        texts[texts.Length - 3].text = "Allomancer mass: " + allomancer.Mass + "kg";
        texts[texts.Length - 2].text = "Coin mass: " + coin.MagneticMass + "kg";
    }

    private void Update() {
        if (SettingsMenu.settingsData.anchoredBoost == 1) {
            texts[texts.Length - 1].text = "Allomantic Normal Force";

            if (allomancer.LastNetForceOnAllomancer.magnitude > 130) {
                texts[3].text = TextCodes.Red(HUD.RoundStringToSigFigs(allomancer.LastNetForceOnAllomancer.magnitude, 3));
            } else {
                texts[3].text = HUD.RoundStringToSigFigs(allomancer.LastNetForceOnAllomancer.magnitude, 3);
            }
        } else {
            texts[texts.Length - 1].text = "Exponential w/ Velocity factor";

            if (allomancer.LastNetForceOnAllomancer.magnitude > 10) {
                texts[3].text = TextCodes.Red(HUD.RoundStringToSigFigs(allomancer.LastNetForceOnAllomancer.magnitude, 3));
            } else {
                texts[3].text = HUD.RoundStringToSigFigs(allomancer.LastNetForceOnAllomancer.magnitude, 3);
            }
        }

        texts[4].text = HUD.RoundStringToSigFigs(allomancer.GetComponent<Rigidbody>().velocity.magnitude, 3);

        if (coin.GetComponent<Rigidbody>().velocity.magnitude < .001f)
            texts[5].text = "0";
        else
            texts[5].text = HUD.RoundStringToSigFigs(coin.GetComponent<Rigidbody>().velocity.magnitude, 3);


        //texts[3].text += "N";
        //texts[4].text += "m/s";
        //texts[5].text += "m/s";
    }
}
