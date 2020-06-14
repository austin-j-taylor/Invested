using UnityEngine;
using System.Collections;

// Challenge where the player must go through a series of rings in a certain amount of time.
public class Challenge_TimeTrial : Challenge {

    private TimeTrialRing[] rings;
    private AudioSource[] sources;

    protected override void Start() {
        base.Start();
        challengeType = "Time Trial: ";
        challengeDescription = "Current record: " + HUD.TimeMMSSMS(PlayerDataController.GetTime(trialDataName)) + "\n\n" + challengeDescription;
        if (failureObjects.Length > 0)
            challengeDescription = challengeDescription + TextCodes.Red("\n\tThe Floor is Lava: Touching the ground fails the challenge.");

        rings = transform.Find("Rings").GetComponentsInChildren<TimeTrialRing>();
        HideAllRings();

        sources = GetComponents<AudioSource>();
    }

    protected override void IntroduceChallenge() {
        base.IntroduceChallenge();
        // Make all rings visible
        ShowAllRings();
    }
    public override void LeaveChallenge() {
        base.LeaveChallenge();
        sources[1].Stop();
        HideAllRings();
    }
    public override void StartChallenge() {
        base.StartChallenge();
        HideAllRings();
        ClearAllRings();
        // Set opacity of next few rings
        SetRingOpacity(0);
        StartCoroutine(Countdown());
    }
    protected override void CompleteChallenge() {
        base.CompleteChallenge();
        sources[1].Stop();
        sources[2].Play();
    }

    protected IEnumerator Countdown() {
        CameraController.UsingCinemachine = true;
        Player.CanControl = false;
        sources[1].Stop();

        double recordTime = PlayerDataController.GetTime(trialDataName);
        HUD.MessageOverlayCinematic.FadeIn("Record: " + HUD.TimeMMSSMS(recordTime));
        yield return new WaitForSeconds(3);
        sources[0].Play();
        HUD.MessageOverlayCinematic.SetText("3");
        yield return new WaitForSeconds(1);
        HUD.MessageOverlayCinematic.SetText("2");
        yield return new WaitForSeconds(1);
        HUD.MessageOverlayCinematic.SetText("1");
        yield return new WaitForSeconds(1);
        HUD.MessageOverlayCinematic.SetText("START");
        sources[1].Play();

        Player.CanControl = true;
        CameraController.UsingCinemachine = false;

        StartCoroutine(TimeTrial(recordTime));
        StartCoroutine(SpikeTracer(recordTime));
    }
    protected virtual IEnumerator TimeTrial(double recordTime) {

        int ringIndex = 0;
        double raceTime = 0;
        do {
            // Set opacity of next few rings
            SetRingOpacity(ringIndex);
            rings[ringIndex].GetComponent<Collider>().enabled = true;
            while (!rings[ringIndex].Passed) {
                raceTime += Time.deltaTime;
                HUD.MessageOverlayCinematic.SetText(HUD.TimeMMSSMS(raceTime));
                yield return null;
            }
            ringIndex++;
        } while (ringIndex < rings.Length);
        CompleteChallenge();

    }

    protected IEnumerator DisplayResults(double raceTime, double recordTime) {
        Debug.Log("Time: " + HUD.TimeMMSSMS(raceTime));
        if (raceTime < recordTime) {
            HUD.MessageOverlayCinematic.FadeIn("Time: " + HUD.TimeMMSSMS(raceTime) + TextCodes.Blue(" (new record!)"));
            PlayerDataController.SetTimeTrial(trialDataName, raceTime);
        } else {
            HUD.MessageOverlayCinematic.FadeIn("Time: " + HUD.TimeMMSSMS(raceTime));
        }
        yield return new WaitForSeconds(5);
        HUD.MessageOverlayCinematic.FadeOut();
    }

    // Makes the spike follow the path of the rings
    protected virtual IEnumerator SpikeTracer(double recordTime) {

        double progress = 0;
        // Spike starts at where it is now and goes through the rings
        int numPoints = 3 * (1 + rings.Length) - 2;
        Vector3[] points = new Vector3[numPoints];
        float speed = (float)recordTime / rings.Length;
        // set start point
        points[0] = spike.transform.position;
        points[1] = spike.transform.position + spike.transform.forward;
        // set middle points
        int pointIndex = 2, ringIndex;
        for(ringIndex = 0; ringIndex < rings.Length - 1; ringIndex++) {
            points[pointIndex++] = rings[ringIndex].transform.position - rings[ringIndex].transform.forward * speed;
            points[pointIndex++] = rings[ringIndex].transform.position;
            points[pointIndex++] = rings[ringIndex].transform.position + rings[ringIndex].transform.forward * speed;
        }
        // set end point
        points[pointIndex++] = rings[ringIndex].transform.position - rings[ringIndex].transform.forward * speed;
        points[pointIndex] = rings[ringIndex].transform.position;
        spikeSpline.SetPoints(points);
        // follow the curve
        while (progress < 1 && !Completed) {
            spikeSpline.FollowCurve(spike.transform, (float)progress, true);
            progress += Time.deltaTime / recordTime;
            yield return null;
        }
    }
    private void ClearAllRings() {
        for (int i = 0; i < rings.Length; i++) {
            rings[i].Clear();
        }
    }
    private void HideAllRings() {
        for (int i = 0; i < rings.Length; i++) {
            rings[i].Hide();
        }
    }
    private void ShowAllRings() {
        for (int i = 0; i < rings.Length; i++) {
            rings[i].Show();
        }
    }
    private void SetRingOpacity(int ringIndex) {
        for (int i = ringIndex; i < ringIndex + 4 && i < rings.Length; i++) {
            Renderer rend = rings[i].GetComponent<Renderer>();
            Color col = rend.material.color;
            col.a = 1 - (i - ringIndex) / 3f;
            rend.material.color = col;
        }
    }
}
