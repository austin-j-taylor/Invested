using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Simulation_coinGround : MonoBehaviour {

    //private float timeToReset;
    private NonPlayerPushPullController allomancerTop;
    private NonPlayerPushPullController allomancerBottom;
    private Magnetic coinTop;
    private Magnetic coinBottom;

    private float counter;
    private float groundHeight;

    private Text[] texts;
    private const int right = 20;

    void Start() {
        allomancerTop = transform.Find("AllomancerTop").GetComponent<NonPlayerPushPullController>();
        coinTop = transform.Find("CoinTop").GetComponentInChildren<Magnetic>();
        allomancerBottom = transform.Find("AllomancerBottom").GetComponent<NonPlayerPushPullController>();
        coinBottom = transform.Find("CoinBottom").GetComponentInChildren<Magnetic>();

        counter = 0;
        groundHeight = transform.Find("CoinWall").transform.position.y + .5f;


        allomancerTop.PushTargets.MaxRange = -1;
        allomancerTop.AddPushTarget(coinTop);
        allomancerTop.SteelBurnRateTarget = 1;
        allomancerBottom.PushTargets.MaxRange = -1;
        allomancerBottom.AddPushTarget(coinBottom);
        allomancerBottom.SteelBurnRateTarget = 1;

        texts = GameObject.FindGameObjectWithTag("Canvas").transform.Find("Simulations").Find("coinGround").GetComponentsInChildren<Text>();

        if (SettingsMenu.settingsData.anchoredBoost == 1) {
            texts[texts.Length - 6].text = "Allomantic Normal Force";
        } else if (SettingsMenu.settingsData.anchoredBoost == 2) {
            texts[texts.Length - 6].text = "Exponential w/ Velocity factor";
            switch (SettingsMenu.settingsData.exponentialWithVelocitySignage) {
                case 0: {
                        texts[texts.Length - 5].text = "Relationship 1 (Both Directions Decrease)";
                        break;
                    }
                case 1: {
                        texts[texts.Length - 5].text = "Relationship 2/3 (Moving Towards Decreases)";
                        break;
                    }
                case 2: {
                        texts[texts.Length - 5].text = "Relationship 3/4 (Moving Away Decreases)";
                        break;
                    }
                case 3: {
                        texts[texts.Length - 5].text = "Relationship 6/7 (Symmetrical)";
                        break;
                    }
            }
        } else if (SettingsMenu.settingsData.anchoredBoost == 3) {
            texts[texts.Length - 6].text = "Distributed Power";
        } else {
            texts[texts.Length - 6].text = "No Anchored Boost";
        }

        if (SettingsMenu.settingsData.forceDistanceRelationship == 0) {
            texts[texts.Length - 4].text = "Linear Distance";
        } else if (SettingsMenu.settingsData.forceDistanceRelationship == 1) {
            texts[texts.Length - 4].text = "Inverse Square Distance";
        } else if (SettingsMenu.settingsData.forceDistanceRelationship == 2) {
            texts[texts.Length - 4].text = "Exponential w/ Distance";
        }

        texts[texts.Length - 3].text = "Time scale: " + HUD.RoundStringToSigFigs(Time.timeScale);
        texts[texts.Length - 2].text = "Allomancer mass: " + allomancerTop.Mass + "kg";
        texts[texts.Length - 1].text = "Coin mass: " + coinTop.MagneticMass + "kg";
    }

    private void FixedUpdate() {
        if (!PauseMenu.IsPaused) {

            if (counter > 2) {
                Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
                foreach (Rigidbody rb in rbs) {
                    rb.useGravity = true;
                }
                allomancerTop.SteelPushing = true;
                allomancerTop.SteelBurnRateTarget = 1;
                allomancerBottom.SteelPushing = true;
                allomancerBottom.SteelBurnRateTarget = 1;
            } else {
                counter += Time.deltaTime / Time.timeScale;
            }

            // Lower the timescale until the coin hits the ground
            if (coinTop.transform.position.y - groundHeight - .125f > .01f) {
                Time.timeScale = .05f;
                Time.fixedDeltaTime = Time.timeScale / 60;
            } else {
                Time.timeScale = SettingsMenu.settingsData.timeScale;
                Time.fixedDeltaTime = Time.timeScale / 60;
            }
            texts[texts.Length - 3].text = "Time scale: " + HUD.RoundStringToSigFigs(Time.timeScale);



            float threshold = 0;
            if (SettingsMenu.settingsData.anchoredBoost == 1) {
                threshold = 300;
            } else if (SettingsMenu.settingsData.anchoredBoost == 2) {
                threshold = 50;
            } else {
                threshold = 8000;
            }

            // Force

            if (allomancerTop.LastNetForceOnAllomancer.magnitude < .01f) {
                texts[0].text = "0";
                texts[1].text = "0";
            } else {
                if (allomancerTop.LastNetForceOnAllomancer.magnitude > threshold) {
                    texts[0].text = TextCodes.Red(HUD.RoundStringToSigFigs(allomancerTop.LastNetForceOnAllomancer.magnitude, 3));
                } else {
                    texts[0].text = HUD.RoundStringToSigFigs(allomancerTop.LastNetForceOnAllomancer.magnitude, 3);
                }
                texts[1].text = HUD.AllomanticSumString(allomancerTop.LastAllomanticForce, allomancerTop.LastAnchoredPushBoost, allomancerTop.Mass, 3);
            }

            if (allomancerBottom.LastNetForceOnAllomancer.magnitude < .01f) {
                texts[0 + right].text = "0";
                texts[1 + right].text = "0";
            } else {
                if (allomancerBottom.LastNetForceOnAllomancer.magnitude > threshold) {
                    texts[0 + right].text = TextCodes.Red(HUD.RoundStringToSigFigs(allomancerBottom.LastNetForceOnAllomancer.magnitude, 3));
                } else {
                    texts[0 + right].text = HUD.RoundStringToSigFigs(allomancerBottom.LastNetForceOnAllomancer.magnitude, 3);
                }
                texts[1 + right].text = HUD.AllomanticSumString(allomancerBottom.LastAllomanticForce, allomancerBottom.LastAnchoredPushBoost, allomancerBottom.Mass, 3);
            }


            // Velocity

            if (allomancerTop.GetComponent<Rigidbody>().velocity.magnitude < .01f) {
                texts[2].text = "0";
            } else {
                texts[2].text = HUD.RoundStringToSigFigs(allomancerTop.GetComponent<Rigidbody>().velocity.magnitude * Mathf.Sign(Vector3.Dot(allomancerTop.GetComponent<Rigidbody>().velocity, Vector3.up)), 2);
            }
            if (coinTop.GetComponent<Rigidbody>().velocity.magnitude < .01f) {
                texts[4].text = "0";
            } else {
                texts[4].text = HUD.RoundStringToSigFigs(coinTop.GetComponent<Rigidbody>().velocity.magnitude * Mathf.Sign(Vector3.Dot(coinTop.GetComponent<Rigidbody>().velocity, Vector3.up)), 2);
            }

            if (allomancerBottom.GetComponent<Rigidbody>().velocity.magnitude < .01f) {
                texts[2 + right].text = "0";
            } else {
                texts[2 + right].text = HUD.RoundStringToSigFigs(allomancerBottom.GetComponent<Rigidbody>().velocity.magnitude * Mathf.Sign(Vector3.Dot(allomancerBottom.GetComponent<Rigidbody>().velocity, Vector3.up)), 2);
            }
            if (coinBottom.GetComponent<Rigidbody>().velocity.magnitude < .01f) {
                texts[4 + right].text = "0";
            } else {
                texts[4 + right].text = HUD.RoundStringToSigFigs(coinBottom.GetComponent<Rigidbody>().velocity.magnitude * Mathf.Sign(Vector3.Dot(coinBottom.GetComponent<Rigidbody>().velocity, Vector3.up)), 2);
            }


            // Distance
            if (allomancerTop.transform.position.y - groundHeight - .5f < .01f)
                texts[3].text = "0";
            else
                texts[3].text = HUD.RoundStringToSigFigs(allomancerTop.transform.position.y - groundHeight - .5f, 3);
            if (coinTop.transform.position.y - groundHeight - .125f < .01f)
                texts[5].text = "0";
            else
                texts[5].text = HUD.RoundStringToSigFigs(coinTop.transform.position.y - groundHeight - .125f, 3);
            if (allomancerBottom.transform.position.y - groundHeight - .5f < .01f)
                texts[3 + right].text = "0";
            else
                texts[3 + right].text = HUD.RoundStringToSigFigs(allomancerBottom.transform.position.y - groundHeight - .5f, 3);
            if (coinBottom.transform.position.y - groundHeight - .125f < .01f)
                texts[5 + right].text = "0";
            else
                texts[5 + right].text = HUD.RoundStringToSigFigs(coinBottom.transform.position.y - groundHeight - .125f, 3);


            // EwV factor
            if (SettingsMenu.settingsData.anchoredBoost == 2) {
                texts[18].text = "e^-v/V Factor:";

                if (allomancerTop.LastAnchoredPushBoost.magnitude / allomancerTop.LastAllomanticForce.magnitude < .0001f) {
                    texts[19].text = "0%";
                } else {
                    string percent = HUD.RoundStringToSigFigs(100 * allomancerTop.LastAnchoredPushBoost.magnitude / allomancerTop.LastAllomanticForce.magnitude) + "%";

                    if (Vector3.Dot(allomancerTop.LastAnchoredPushBoost, allomancerTop.LastAllomanticForce) > 0) {
                        percent = TextCodes.Blue("+" + percent);
                    } else {
                        if (allomancerTop.LastAnchoredPushBoost.magnitude / allomancerTop.LastAllomanticForce.magnitude > .5f) {
                            percent = TextCodes.Red("-" + percent);
                        } else {
                            percent = ("-" + percent);
                        }
                    }
                    texts[19].text = percent;
                }

                texts[18 + right].text = "e^-v/V Factor:";

                if (allomancerBottom.LastAnchoredPushBoost.magnitude / allomancerBottom.LastAllomanticForce.magnitude < .0001f) {
                    texts[19 + right].text = "0%";
                } else {
                    string percent = HUD.RoundStringToSigFigs(100 * allomancerBottom.LastAnchoredPushBoost.magnitude / allomancerBottom.LastAllomanticForce.magnitude) + "%";

                    if (Vector3.Dot(allomancerBottom.LastAnchoredPushBoost, allomancerBottom.LastAllomanticForce) > 0) {
                        percent = TextCodes.Blue("+" + percent);
                    } else {
                        if (allomancerBottom.LastAnchoredPushBoost.magnitude / allomancerBottom.LastAllomanticForce.magnitude > .5f) {
                            percent = TextCodes.Red("-" + percent);
                        } else {
                            percent = ("-" + percent);
                        }
                    }
                    texts[19 + right].text = percent;
                }



            } else {
                texts[18].text = "";
                texts[19].text = "";
                texts[18 + right].text = "";
                texts[19 + right].text = "";
            }
        }
    }
}
