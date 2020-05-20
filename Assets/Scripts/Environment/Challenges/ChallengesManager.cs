using UnityEngine;
using System.Collections;

// Manages the challenges in a level that provide Spikes and unlock the Harmony Target
public class ChallengesManager : MonoBehaviour {

    [SerializeField]
    private HarmonyTarget harmonyTarget = null;

    public void CompleteChallege(SpikeSpline spline, GameObject spike) {
        StartCoroutine(MoveSpikeToTarget(spline, spike));
    }

    private IEnumerator MoveSpikeToTarget(SpikeSpline spline, GameObject spike) {

        float progress = 0;
        // Spike starts at where it is now
        Vector3[] points = new Vector3[4];
        points[0] = spike.transform.position;
        points[1] = spike.transform.position + spike.transform.forward;
        Vector3 position = harmonyTarget.GetNextSpikePosition();
        points[3] = position;
        points[2] = harmonyTarget.GetNextSpikeAngle();
        spline.SetPoints(points);
        // Spike ends at the harmony target's next spike position (needs to be constantly updated, as it moves in world space)
        while (progress < 1) {
            position = harmonyTarget.GetNextSpikePosition();
            spline.SetControlPoint(3, position);
            spline.SetControlPoint(2, harmonyTarget.GetNextSpikeAngle());
            spline.FollowCurve(spike.transform, progress, true);
            progress += Time.deltaTime / spline.AnimationTime;
            yield return null;
        }
        spike.SetActive(false);
        harmonyTarget.AddSpike();

    }
}
