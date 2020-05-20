using UnityEngine;
using System.Collections;

// Represents a challenge for the player to complete, like "go through these rings."
public class Challenge : MonoBehaviour {

    [SerializeField]
    ChallengesManager manager = null;
    [SerializeField]
    protected SpikeSpline spikeSpline = null;
    [SerializeField]
    protected GameObject spike = null;
    
    public bool Completed { get; private set; }

    protected virtual void StartChallenge() {

    }
    protected virtual void CompleteChallenge() {
        Completed = true;
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
