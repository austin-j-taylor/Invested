using UnityEngine;
using UnityEngine.UI;

public class Simulation_coinWall : Simulation {

    //private float timeToReset;
    private NonPlayerPushPullController allomancer;
    private Magnetic coin;
    private Rigidbody coinWall;
    private Rigidbody alloWall;

    private float counter;
    private bool cleared;
    //private Vector3 wallLastVelocity = Vector3.zero;

    private Text[] texts;

    private void Awake() {
        ResetTime = 14;
    }

    public override void StartSimulation() {
        base.StartSimulation();

        allomancer = GetComponentInChildren<NonPlayerPushPullController>();
        coin = GetComponentInChildren<Magnetic>();
        coinWall = transform.Find("CoinWall").GetComponent<Rigidbody>();
        alloWall = transform.Find("AlloWall").GetComponent<Rigidbody>();

        counter = 0;
        cleared = false;

        allomancer.PushTargets.MaxRange = -1;
        allomancer.AddPushTarget(coin);
        texts = HUDSimulations.CoinWall.GetComponentsInChildren<Text>();
        
        texts[texts.Length - 8].text = "Wall: " + TextCodes.LightBlue("Anchored");
        texts[texts.Length - 7].text = "Allomancer: " + TextCodes.Gray("Unanchored");
        texts[texts.Length - 6].text = "Coin: " + TextCodes.Gray("Unanchored");
        texts[texts.Length - 5].text = "Wall: " + TextCodes.LightBlue("Anchored");


        if (SettingsMenu.settingsAllomancy.forceDistanceRelationship == 0) {
            texts[texts.Length - 9].text = "Linear Distance Relationship";
        } else if(SettingsMenu.settingsAllomancy.forceDistanceRelationship == 1) {
            texts[texts.Length - 9].text = "Inverse Square Distance Relationship";
        } else if(SettingsMenu.settingsAllomancy.forceDistanceRelationship == 2) {
            texts[texts.Length - 9].text = "Exponential w/ Distance Relationship";
        }

        texts[texts.Length - 3].text = "Time scale: " + HUD.RoundStringToSigFigs(Time.timeScale);
        texts[texts.Length - 2].text = "Allomancer mass: " + allomancer.Mass + "kg";
        texts[texts.Length - 1].text = "Coin mass: " + coin.MagneticMass + "kg";
    }

    private void FixedUpdate() {
        if (allomancer && allomancer.HasPushTarget && !PauseMenu.IsPaused) {
            counter += Time.deltaTime;
            if (counter > .5f) {
                allomancer.SteelPushing = true;
                allomancer.SteelBurnPercentageTarget = 1;
                if(counter > 4 && counter < 6) {
                    texts[texts.Length - 5].text = "Wall: " + TextCodes.Gray("Unanchored");
                    texts[texts.Length - 6].text = "Coin: " + TextCodes.Blue("Partially Anchored");
                    coinWall.isKinematic = false;
                } else if(counter > 6 && counter < 8) {
                    texts[texts.Length - 5].text = "Wall: Removed";
                    texts[texts.Length - 6].text = "Coin: " + TextCodes.Gray("Unanchored");
                    coinWall.gameObject.SetActive(false);
                } else if(counter > 8 && counter < 10) {
                    alloWall.gameObject.SetActive(false);
                    texts[texts.Length - 8].text = "Wall: Removed";
                } else if(counter > 10 && counter < 12 && !cleared) {
                    cleared = true;
                    coinWall.gameObject.SetActive(true);
                    coinWall.isKinematic = true;
                    coinWall.transform.localPosition = new Vector3(0, 0, -4f);
                    coin.transform.localPosition = new Vector3(0, 0, 3.35f);
                    coin.Clear();   
                    texts[texts.Length - 5].text = "Wall: " + TextCodes.LightBlue("Anchored");
                }
            }


            float threshold = 0;
            if (SettingsMenu.settingsAllomancy.anchoredBoost == 1) {
                threshold = 300;
            } else if (SettingsMenu.settingsAllomancy.anchoredBoost == 2) {
                threshold = 50;
            } else {
                threshold = 8000;
            }

            if (allomancer.LastNetForceOnAllomancer.magnitude < .01f) {
                texts[3].text = "0";
                texts[9].text = "0";
            } else {
                if (allomancer.LastNetForceOnAllomancer.magnitude > threshold) {
                    texts[3].text = TextCodes.Red(HUD.ForceString(allomancer.LastNetForceOnAllomancer.magnitude, allomancer.Mass, 3));
                } else {
                    texts[3].text = HUD.ForceString(allomancer.LastNetForceOnAllomancer.magnitude, allomancer.Mass, 3);
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
                texts[texts.Length - 6].text = "Coin: " + TextCodes.LightBlue("Anchored");
            } else {
                if (counter < 2) {
                    texts[texts.Length - 6].text = "Coin: " + TextCodes.Gray("Unanchored");
                }
                texts[5].text = HUD.RoundStringToSigFigs(coin.GetComponent<Rigidbody>().velocity.magnitude, 2);
            }
            if(SettingsMenu.settingsAllomancy.anchoredBoost == 2) {
                texts[10].text = "e^-v/V Factor:";
                texts[11].text = "";
                texts[12].text = "";
                texts[16].text = "";
                texts[17].text = "";
                texts[18].text = "";

                string percent = HUD.RoundStringToSigFigs(100 * allomancer.LastAnchoredPushBoost.magnitude / allomancer.LastAllomanticForce.magnitude) + "%";

                if (Vector3.Dot(allomancer.LastAnchoredPushBoost, allomancer.LastAllomanticForce) > 0) {
                    percent = TextCodes.Blue("+" + percent);
                } else {
                    if(allomancer.LastAnchoredPushBoost.magnitude / allomancer.LastAllomanticForce.magnitude > .5f) {
                        percent = TextCodes.Red("-" + percent);
                    } else {
                        percent = ("-" + percent);
                    }
                }
                texts[13].text = percent;

            } else if(SettingsMenu.settingsAllomancy.anchoredBoost == 3) {
                texts[10].text = "Allomancer ΔKE:";
                texts[11].text = "Coin ΔKE:";
                texts[12].text = "Total ΔKE:";
                texts[16].text = "J";
                texts[17].text = "J";
                texts[18].text = "J";


                float alloEnergy = .5f * allomancer.Mass * (allomancer.LastAllomancerVelocity - allomancer.GetComponent<Rigidbody>().velocity).sqrMagnitude / (Time.timeScale * Time.timeScale);
                float coinEnergy = .5f * coin.MagneticMass * (coin.LastVelocity - coin.GetComponent<Rigidbody>().velocity).sqrMagnitude / (Time.timeScale * Time.timeScale);
                if (alloEnergy < .001f) {
                    alloEnergy = 0;

                    texts[13].text = "0";
                    if (coinEnergy > .001f) {
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
                //Debug.Log(.5f * coinWall.GetComponent<Rigidbody>().mass * (wallLastVelocity - coinWall.GetComponent<Rigidbody>().velocity).sqrMagnitude / (Time.timeScale * Time.timeScale));
                //wallLastVelocity = coinWall.GetComponent<Rigidbody>().velocity;
                if (alloEnergy + coinEnergy < 99)
                    texts[15].text = HUD.RoundStringToSigFigs(alloEnergy + coinEnergy);
                else
                    texts[15].text = TextCodes.Red(HUD.RoundStringToSigFigs(alloEnergy + coinEnergy));
            }
        }
    }
    protected override void Update() {
        if (SettingsMenu.settingsAllomancy.anchoredBoost == 1) {
            texts[texts.Length - 4].text = "Allomantic Normal Force";
            //desiredTimeScale = 1;
        } else if (SettingsMenu.settingsAllomancy.anchoredBoost == 2) {
            texts[texts.Length - 4].text = "Exponential w/ Velocity factor";
            //desiredTimeScale = 1;
        } else {
            texts[texts.Length - 4].text = "Distributed Power";
            //desiredTimeScale = .2f;
        }
        // This is what messes up the DP's energy distribution
        //Time.fixedDeltaTime = Time.timeScale * 1 / 60f;

        base.Update();

        texts[texts.Length - 3].text = "Time scale: " + HUD.RoundStringToSigFigs(Time.timeScale);
    }
}
