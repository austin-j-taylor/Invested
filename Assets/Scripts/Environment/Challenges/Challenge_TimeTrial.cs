using UnityEngine;
using System.Collections;

// Challenge where the player must go through a series of rings in a certain amount of time.
public class Challenge_TimeTrial : Challenge {

    [SerializeField]
    private string trialName = "";

    private TimeTrialRing[] rings;

    protected override void Start() {
        base.Start();
        challengeName = "Time Trial: " + challengeName;
        challengeDescription += "\n\nCurrent record: " + HUD.TimeMMSSMS(PlayerDataController.GetTime(trialName));

        rings = transform.Find("Rings").GetComponentsInChildren<TimeTrialRing>();
        HideAllRings();
    }

    protected override void IntroduceChallenge() {
        base.IntroduceChallenge();
        // Make all rings visible
        ShowAllRings();
    }
    public override void LeaveChallenge() {
        base.LeaveChallenge();
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

    private IEnumerator Countdown() {
        CameraController.UsingCinemachine = true;
        Player.CanControl = false;

        double recordTime = PlayerDataController.GetTime(trialName);
        HUD.MessageOverlayCinematic.FadeIn("Record: " + HUD.TimeMMSSMS(recordTime));
        yield return new WaitForSeconds(3);

        HUD.MessageOverlayCinematic.FadeIn("3");
        yield return new WaitForSeconds(1);
        HUD.MessageOverlayCinematic.FadeIn("2");
        yield return new WaitForSeconds(1);
        HUD.MessageOverlayCinematic.FadeIn("1");
        yield return new WaitForSeconds(1);
        HUD.MessageOverlayCinematic.FadeIn("START");

        StartCoroutine(TimeTrial(recordTime));
        StartCoroutine(SpikeTracer(recordTime));

        yield return new WaitForSeconds(2);
        HUD.MessageOverlayCinematic.FadeOut();
    }
    private IEnumerator TimeTrial(double recordTime) {

        Player.CanControl = true;
        CameraController.UsingCinemachine = false;

        int ringIndex = 0;
        double raceTime = 0;
        Debug.Log("Starting challenge with record: " + HUD.TimeMMSSMS(recordTime));
        do {
            // Set opacity of next few rings
            SetRingOpacity(ringIndex);
            rings[ringIndex].GetComponent<Collider>().enabled = true;
            while (!rings[ringIndex].Passed) {
                raceTime += Time.deltaTime;
                yield return null;
            }
            ringIndex++;
        } while (ringIndex < rings.Length);
        StartCoroutine(DisplayResults(raceTime, recordTime));

        CompleteChallenge();
    }

    private IEnumerator DisplayResults(double raceTime, double recordTime) {
        Debug.Log("Time: " + HUD.TimeMMSSMS(raceTime));
        if (raceTime < recordTime) {
            HUD.MessageOverlayCinematic.FadeIn("Time: " + HUD.TimeMMSSMS(raceTime) + TextCodes.Blue("(new record!)"));
            PlayerDataController.SetTimeTrial(trialName, raceTime);
        } else {
            HUD.MessageOverlayCinematic.FadeIn("Time: " + HUD.TimeMMSSMS(raceTime));
        }
        yield return new WaitForSeconds(5);
        HUD.MessageOverlayCinematic.FadeOut();
    }

    // Makes the spike follow the path of the rings
    private IEnumerator SpikeTracer(double recordTime) {

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
