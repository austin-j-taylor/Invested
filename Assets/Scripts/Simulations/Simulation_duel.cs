using UnityEngine;
using UnityEngine.UI;

public class Simulation_duel : Simulation {

    //private float timeToReset;
    private NonPlayerPushPullController[] allomancers;
    private Magnetic[] spheres;

    private Text[] texts;

    private void Awake() {
        ResetTime = 3;
    }

    public override void StartSimulation() {
        base.StartSimulation();

        allomancers = GetComponentsInChildren<NonPlayerPushPullController>();
        spheres = GetComponentsInChildren<Magnetic>();

        for (int i = 0; i < allomancers.Length; i++) {
            allomancers[i].AddPushTarget(spheres[i / 2]);
            allomancers[i].SteelPushing = true;
            allomancers[i].SteelBurnPercentageTarget = 1;
            allomancers[i].PullTargets.MaxRange = 50;
            allomancers[i].PushTargets.MaxRange = 50;
        }
        texts = HUDSimulations.Duel.GetComponentsInChildren<Text>();

        //Time.timeScale = 1f;
        //Time.fixedDeltaTime = Time.timeScale * 1 / 60f;


        //allomancers[7].Strength = 1.2f;
        //allomancers[8].Strength = 1.2f;

        allomancers[1].Strength = 1.5f;
        allomancers[7].Strength = 1.5f;
        allomancers[9].Strength = 1.5f;
    }

    protected override void Update() {
        base.Update();
        if (allomancers != null) {
            for (int i = 0; i < 10; i++) {
                string str = "";
                if (i % 2 == 0)
                    str = "Pair " + (i / 2 + 1) + ":\n";
                else
                    str = "\n";
                texts[i].text = str + "mass = " + allomancers[i].Mass + "kg\nStrength = " + allomancers[i].Strength + "\nForce: " + HUD.AllomanticSumString(allomancers[i].LastAllomanticForce, allomancers[i].LastAnchoredPushBoost, allomancers[i].Mass, 2);
            }
        }
    }
}
