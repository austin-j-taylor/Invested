using UnityEngine;
using System.Collections;

// Challenge to hit all the targets
public class Challenge_BreakTargets : Challenge_TimeTrial {


    protected override void Start() {
        base.Start();
        challengeType = "Hit the Targets: ";
    }

    public override void StartChallenge() {
        base.StartChallenge();
        if (failureObjects.Length > 0)
            StartCoroutine(Countdown());
        Resettable resettable = challengeObjects.GetComponent<Resettable>();
        resettable.Activate();
    }

    protected override IEnumerator TimeTrial(double recordTime) {
        Target[] targets = challengeObjects.GetComponentsInChildren<Target>();
        double raceTime = 0;
        bool done = false;
        while (!done) {
            done = true;
            for (int i = 0; i < targets.Length; i++) {
                done = done & targets[i].On;
            }
            raceTime += Time.deltaTime;
            HUD.MessageOverlayDescriptive.SetHeader(HUD.TimeMMSSMS(raceTime));
            yield return null;
        }
        HUD.MessageOverlayDescriptive.Clear();
        CompleteChallenge();
        DisplayResults(raceTime, recordTime);
    }

    protected override IEnumerator SpikeTracer(double recordTime) {
        yield break;
    }
}