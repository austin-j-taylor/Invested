using UnityEngine;
using System.Collections;
using Cinemachine;

// Represents a challenge for the player to complete, like "go through these rings."
public class Challenge : MonoBehaviour {

    private CinemachineVirtualCamera vcam;

    public string challengeName = "";
    [SerializeField]
    ChallengesManager manager = null;

    private Collider introduceTrigger;
    protected SpikeSpline spikeSpline;
    protected GameObject spike;
    private Transform startPosition;
    private Vector3 spikeStartPosition;
    private Quaternion spikeStartRotation;
    
    public bool Completed { get; private set; }

    protected virtual void Start() {
        introduceTrigger = transform.Find("IntroductionTrigger").GetComponent<Collider>();
        introduceTrigger.gameObject.AddComponent<ChallengeTrigger>().parent = this;
        spike = transform.Find("Spike").gameObject;
        spikeSpline = transform.Find("SpikeSpline").GetComponent<SpikeSpline>();
        startPosition = transform.Find("StartPosition");
        spikeStartPosition = spike.transform.position;
        spikeStartRotation = spike.transform.rotation;
        vcam = GetComponentInChildren<CinemachineVirtualCamera>();
        vcam.enabled = false;
    }

    protected virtual void IntroduceChallenge() {
        introduceTrigger.gameObject.SetActive(false);
        CameraController.UsingCinemachine = true;
        vcam.enabled = true;
        ChallengeMenu.Open(this);
    }
    public virtual void LeaveChallenge() {
        StopAllCoroutines();
        HUD.MessageOverlayCinematic.Clear();
        Player.CanControl = true;

        CameraController.UsingCinemachine = false;
        vcam.enabled = false;
        spike.transform.position = spikeStartPosition;
        spike.transform.rotation = spikeStartRotation;
        GameManager.State = GameManager.GameState.Standard;
        StartCoroutine(EnableColliderAfterDelay());
    }
    public virtual void StartChallenge() {
        StopAllCoroutines();
        HUD.MessageOverlayCinematic.Clear();

        CameraController.UsingCinemachine = false;
        vcam.enabled = false;
        spike.transform.position = spikeStartPosition;
        spike.transform.rotation = spikeStartRotation;
        GameManager.State = GameManager.GameState.Challenge;
        // Move player to start and set camera rotation
        Player.PlayerPewter.Clear();
        Player.PlayerInstance.transform.position = startPosition.position;
        CameraController.Clear();
        CameraController.SetRotation(startPosition.eulerAngles);

    }
    protected virtual void CompleteChallenge() {
        Completed = true;
        manager.CompleteChallege(spikeSpline, spike);
        GameManager.State = GameManager.GameState.Standard;
    }

    private IEnumerator EnableColliderAfterDelay() {
        yield return new WaitForSeconds(2);
        introduceTrigger.gameObject.SetActive(true);
    }

    protected class ChallengeTrigger : MonoBehaviour {

        public Challenge parent;

        private void OnTriggerEnter(Collider other) {
            if (Player.IsPlayerTrigger(other)) {
                parent.IntroduceChallenge();
            }
        }
    }
}
