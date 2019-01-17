using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VolumetricLines;

public class Simulation_duel : MonoBehaviour {

    //private float timeToReset;
    private NonPlayerPushPullController[] allomancers;
    private Magnetic[] spheres;

    private Text[] texts;

    // Use this for initialization
    void Start() {
        allomancers = GetComponentsInChildren<NonPlayerPushPullController>();
        spheres = GetComponentsInChildren<Magnetic>();
        
        for(int i = 0; i < allomancers.Length; i++) {
            allomancers[i].AddPushTarget(spheres[i / 2]);
            allomancers[i].SteelPushing = true;
            allomancers[i].SteelBurnRateTarget = 1;
            allomancers[i].PullTargets.MaxRange = 50;
            allomancers[i].PushTargets.MaxRange = 50;
        }
        texts = GameObject.FindGameObjectWithTag("Canvas").transform.Find("Simulations").Find("duel").GetComponentsInChildren<Text>();


        //allomancers[7].Strength = 1.2f;
        //allomancers[8].Strength = 1.2f;

        allomancers[1].Strength = 1.5f;
        allomancers[7].Strength = 1.5f;
        allomancers[9].Strength = 1.5f;
    }

    private void Update() {
        for (int i = 0; i < 10; i++) {
            string str = "";
            if (i % 2 == 0)
                str = "Pair " + (i / 2 + 1) + ":\n";
            else
                str = "\n";
            texts[i].text = str + "mass = " + allomancers[i].Mass + "kg\nStrength = " + allomancers[i].Strength + "\nForce: " + HUD.RoundStringToSigFigs(allomancers[i].LastNetForceOnAllomancer.magnitude, 2) + "N";
        }
    }
}
