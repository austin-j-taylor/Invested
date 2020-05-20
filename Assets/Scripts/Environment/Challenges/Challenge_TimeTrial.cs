using UnityEngine;
using System.Collections;

// Challenge where the player must go through a series of rings in a certain amount of time.
public class Challenge_TimeTrial : Challenge {

    [SerializeField]
    private string trialName = "";

    private Collider sphere;
    private TimeTrialRing[] rings;

    private void Start() {
        sphere = GetComponentInChildren<Collider>();
        sphere.gameObject.AddComponent<ChallengeTrigger>().parent = this;
        rings = transform.Find("Rings").GetComponentsInChildren<TimeTrialRing>();
        for(int i = 0; i < rings.Length; i++) {
            rings[i].gameObject.SetActive(false);
        }
    }
    protected override void StartChallenge() {
        base.StartChallenge();
        sphere.gameObject.SetActive(false);
        double recordTime = PlayerDataController.GetTime(trialName);
        StartCoroutine(TimeTrial(recordTime));
        StartCoroutine(SpikeTracer(recordTime));
    } 
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
    private IEnumerator TimeTrial(double recordTime) {

        int ringIndex = 0;
        double raceTime = 0;
        Debug.Log("Starting challenge with record: " + HUD.TimeMMSSMS(recordTime));
        do {
            rings[ringIndex].gameObject.SetActive(true);
            while (!rings[ringIndex].Passed) {
                raceTime += Time.deltaTime;
                yield return null;
            }
            ringIndex++;
        } while (ringIndex < rings.Length);
        
        Debug.Log("Time: " + HUD.TimeMMSSMS(raceTime));
        if(raceTime < recordTime) {
            PlayerDataController.SetTimeTrial(trialName, raceTime);
            Debug.Log("New record! " + HUD.TimeMMSSMS(PlayerDataController.GetTime(trialName)) + " replaces " + HUD.TimeMMSSMS(recordTime));
        }

        CompleteChallenge();
    }

}
