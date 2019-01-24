using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Simulation_coinWall : MonoBehaviour {

    //private float timeToReset;
    private NonPlayerPushPullController allomancer;
    private Magnetic coin;
    private Rigidbody wall;

    private float counter;
    //private Vector3 wallLastVelocity = Vector3.zero;

    private Text[] texts;

    // Use this for initialization
    void Start() {
        allomancer = GetComponentInChildren<NonPlayerPushPullController>();
        coin = GetComponentInChildren<Magnetic>();
        wall = transform.Find("Wall").GetComponent<Rigidbody>();

        counter = 0;

        allomancer.PushTargets.MaxRange = -1;
        allomancer.AddPushTarget(coin);
        texts = GameObject.FindGameObjectWithTag("Canvas").transform.Find("Simulations").Find("coinWall").GetComponentsInChildren<Text>();
        
        texts[texts.Length - 7].text = "Allomancer: " + TextCodes.Gray("Unanchored");
        texts[texts.Length - 6].text = "Coin: " + TextCodes.Gray("Unanchored");
        texts[texts.Length - 5].text = "Wall: " + TextCodes.LightBlue("Anchored");

        if (SettingsMenu.settingsData.anchoredBoost == 1) {
            texts[texts.Length - 4].text = "Allomantic Normal Force";
            Time.timeScale = 1f;
        } else if (SettingsMenu.settingsData.anchoredBoost == 2) {
            texts[texts.Length - 4].text = "Exponential w/ Velocity factor";
            Time.timeScale = 1f;
        } else {
            texts[texts.Length - 4].text = "Distributed Power";
            Time.timeScale = .2f;
        }
        // This is what messes up the DP's energy distribution
        Time.fixedDeltaTime = Time.timeScale * 1 / 60f;

        texts[texts.Length - 3].text = "Time scale: " + Time.timeScale;
        texts[texts.Length - 2].text = "Allomancer mass: " + allomancer.Mass + "kg";
        texts[texts.Length - 1].text = "Coin mass: " + coin.MagneticMass + "kg";
    }

    private void FixedUpdate() {
        if (allomancer.HasPushTarget && !PauseMenu.IsPaused) {
            counter += Time.deltaTime / Time.timeScale;
            if (counter > .5f) {
                allomancer.SteelPushing = true;
                allomancer.SteelBurnRateTarget = 1;
                if (counter > 2) {
                    if (counter > 4) {
                        if(counter > 6) {
                            SceneSelectMenu.ReloadScene();
                        }
                        texts[texts.Length - 5].text = "Wall: Removed";
                        texts[texts.Length - 6].text = "Coin: " + TextCodes.Gray("Unanchored");
                        wall.gameObject.SetActive(false);
                    } else {
                        texts[texts.Length - 5].text = "Wall: " + TextCodes.Gray("Unanchored");
                        texts[texts.Length - 6].text = "Coin: " + TextCodes.Blue("Partially Anchored");
                        wall.isKinematic = false;
                    }
                }
            }

            texts[texts.Length - 3].text = "Time scale: " + Time.timeScale;

            float threshold = 0;
            if (SettingsMenu.settingsData.anchoredBoost == 1) {
                threshold = 300;
            } else if (SettingsMenu.settingsData.anchoredBoost == 2) {
                threshold = 140;
            } else {
                threshold = 8000;
            }

            if (allomancer.LastNetForceOnAllomancer.magnitude < .01f) {
                texts[3].text = "0";
                texts[9].text = "0";
            } else {
                if (allomancer.LastNetForceOnAllomancer.magnitude > threshold) {
                    texts[3].text = TextCodes.Red(HUD.RoundStringToSigFigs(allomancer.LastNetForceOnAllomancer.magnitude, 3));
                } else {
                    texts[3].text = HUD.RoundStringToSigFigs(allomancer.LastNetForceOnAllomancer.magnitude, 3);
                }
                texts[9].text = HUD.AllomanticSumString(allomancer.LastAllomanticForce, allomancer.LastAnchoredPushBoost, allomancer.Mass, 3);
            }

            if (allomancer.GetComponent<Rigidbody>().velocity.magnitude < .01f) {
                texts[4].text = "0";
                texts[texts.Length - 7].text = "Allomancer: " + TextCodes.LightBlue("Anchored");
            } else {
                texts[4].text = HUD.RoundStringToSigFigs(allomancer.GetComponent<Rigidbody>().velocity.magnitude, 2);
                texts[texts.Length - 7].text = "Allomancer: " + TextCodes.Gray("Unanchored");
            }
            if (coin.GetComponent<Rigidbody>().velocity.magnitude < .01f) {
                texts[5].text = "0";
                texts[texts.Length - 5].text = "Coin: " + TextCodes.LightBlue("Anchored");
            } else {
                if (counter < 4) {
                    texts[texts.Length - 5].text = "Coin: " + TextCodes.Gray("Unanchored");
                }
                texts[5].text = HUD.RoundStringToSigFigs(coin.GetComponent<Rigidbody>().velocity.magnitude, 2);
            }

            float alloEnergy = .5f * allomancer.Mass * (allomancer.lastAllomancerVelocity - allomancer.GetComponent<Rigidbody>().velocity).sqrMagnitude / (Time.timeScale * Time.timeScale);
            float coinEnergy = .5f * coin.MagneticMass * (coin.LastVelocity - coin.GetComponent<Rigidbody>().velocity).sqrMagnitude / (Time.timeScale * Time.timeScale);
            if (alloEnergy < .001f) {
                alloEnergy = 0;

                texts[13].text = "0";
                if(coinEnergy > .001f) {
                    texts[14].text = TextCodes.Red(HUD.RoundStringToSigFigs(coinEnergy));
                } else {
                    texts[14].text = "0";
                }
            } else {
                if (coinEnergy > .001f) {
                    texts[13].text = HUD.RoundStringToSigFigs(alloEnergy);
                    texts[14].text = HUD.RoundStringToSigFigs(coinEnergy);
                } else {
                    coinEnergy = 0;

                    texts[13].text = TextCodes.Red(HUD.RoundStringToSigFigs(alloEnergy));
                    texts[14].text = "0";
                }
            }
            //Debug.Log(.5f * wall.GetComponent<Rigidbody>().mass * (wallLastVelocity - wall.GetComponent<Rigidbody>().velocity).sqrMagnitude / (Time.timeScale * Time.timeScale));
            //wallLastVelocity = wall.GetComponent<Rigidbody>().velocity;
            if (alloEnergy + coinEnergy < 99)
                texts[15].text = HUD.RoundStringToSigFigs(alloEnergy + coinEnergy);
            else
                texts[15].text = TextCodes.Red(HUD.RoundStringToSigFigs(alloEnergy + coinEnergy));
        }
    }
}
