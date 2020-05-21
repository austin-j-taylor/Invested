using UnityEngine;
using System.Collections;

// Manages the challenges in a level that provide Spikes and unlock the Harmony Target
public class ChallengesManager : MonoBehaviour {

    [SerializeField]
    private HarmonyTarget harmonyTarget = null;
    private Challenge[] challenges;

    private Challenge currentChallenge;
    private static ChallengesManager instance;

    private void Start() {
        challenges = GetComponentsInChildren<Challenge>();
        instance = this;
    }

    public void IntroduceChallenge(Challenge challenge) {
        instance = this;
        currentChallenge = challenge;
        ChallengeMenu.OpenIntroduction(challenge);
        // Disable all other challenges
        for (int i = 0; i < challenges.Length; i++) {
            if (challenges[i] != challenge) {
                challenges[i].gameObject.SetActive(false);
            }
        }
    }

    public void CompleteChallege(SpikeSpline spline, GameObject spike) {
        StartCoroutine(MoveSpikeToTarget(spline, spike));
        CleanupChallenges();
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

    private void CleanupChallenges() {
        // Turn all challenges back on
        for (int i = 0; i < challenges.Length; i++) {
            challenges[i].gameObject.SetActive(true);
        }
    }

    public static void StartCurrentChallenge() {
        instance.currentChallenge.StartChallenge();
    }
    public static void LeaveCurrentChallenge() {
        instance.currentChallenge.LeaveChallenge();
        instance.CleanupChallenges();
    }
    public static void RestartCurrentChallenge() {
        instance.currentChallenge.StartChallenge();
    }
    public static void FailCurrentChallenge() {
        instance.currentChallenge.FailChallenge();
        ChallengeMenu.OpenFailure();
    }
}
