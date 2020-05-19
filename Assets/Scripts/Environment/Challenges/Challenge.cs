using UnityEngine;
using System.Collections;

// Represents a challenge for the player to complete, like "go through these rings."
public class Challenge : MonoBehaviour {

    [SerializeField]
    ChallengesManager manager = null;
    [SerializeField]
    SpikeSpline spikeSpline = null;
    [SerializeField]
    GameObject spike = null;

    protected virtual void StartChallenge() {

    }
    protected virtual void CompleteChallenge() {
        manager.CompleteChallege(spikeSpline, spike);
    }

    protected class ChallengeTrigger : MonoBehaviour {

        public Challenge parent;

        private void OnTriggerEnter(Collider other) {
            if (Player.IsPlayerTrigger(other)) {
                parent.StartChallenge();
            }
        }
    }
}
