using UnityEngine;
using System.Collections;
using Cinemachine;

// Represents a challenge for the player to complete, like "go through these rings."
public class Challenge : MonoBehaviour {

    protected CinemachineVirtualCamera vcam;

    [SerializeField]
    protected string trialDataName = "";
    [SerializeField]
    private string challengeName = "";
    [TextArea]
    public string challengeDescription = "";
    [SerializeField]
    public bool recommendedIron = true, recommendedSteel = true, recommendedPewter = false, recommendedZinc = false, recommendedCoins = false;
    [SerializeField]
    protected GameObject[] failureObjects = null;
    private string[] failureObjectTags;

    protected string challengeType = "";

    private ChallengesManager manager;
    protected Transform challengeObjects; // objects that will only be active for this challenge
    private Collider introduceTrigger;
    protected SpikeSpline spikeSpline;
    protected GameObject spike;
    private Transform startPosition;
    private Quaternion spikeStartRotation;
    
    public bool Completed { get; private set; }

    protected virtual void Start() {
        manager = GetComponentInParent<ChallengesManager>();
        challengeObjects = transform.Find("ChallengeObjects");
        introduceTrigger = transform.Find("IntroductionTrigger").GetComponent<Collider>();
        introduceTrigger.gameObject.AddComponent<ChallengeTrigger>().parent = this;
        spike = transform.Find("Spike").gameObject;
        spikeSpline = transform.Find("SpikeSpline").GetComponent<SpikeSpline>();
        startPosition = transform.Find("StartPosition");
        spikeStartRotation = spike.transform.rotation;
        vcam = GetComponentInChildren<CinemachineVirtualCamera>();
        vcam.enabled = false;

        challengeObjects.gameObject.SetActive(false);
    }

    protected virtual void IntroduceChallenge() {
        introduceTrigger.gameObject.SetActive(false);
        CameraController.SetCinemachineCamera(vcam);
        manager.IntroduceChallenge(this);

        challengeObjects.gameObject.SetActive(true);
        // Set the tags for all objects that cause failure when touched
        if (failureObjects != null) {
            failureObjectTags = new string[failureObjects.Length];
            for (int i = 0; i < failureObjects.Length; i++) {
                if (failureObjects[i] == null) {
                    Debug.LogError("Error: Failure Object is null", gameObject);
                }
                failureObjectTags[i] = failureObjects[i].tag;
                failureObjects[i].tag = "ChallengeFailure";
            }
        }
    }
    public virtual void LeaveChallenge() {
        CleanupChallenge();

        spike.transform.position = introduceTrigger.transform.position + new Vector3(0, 2.5f, 0);
        spike.transform.rotation = spikeStartRotation;
        StartCoroutine(EnableColliderAfterDelay());
    }
    public virtual void StartChallenge() {
        StopAllCoroutines();
        HUD.MessageOverlayCinematic.Clear();

        spike.transform.position = startPosition.transform.position + new Vector3(0, 2.5f, 0);
        spike.transform.rotation = spikeStartRotation;
        GameManager.SetPlayState(GameManager.GamePlayState.Challenge);
        // Move player to start and set camera rotation
        Player.PlayerPewter.Clear();
        Player.PlayerInstance.transform.position = startPosition.position;
        CameraController.Clear();
        CameraController.SetRotation(startPosition.eulerAngles);

        CameraController.DisableCinemachineCamera(vcam);
        Debug.Log("vcam " + vcam.gameObject + " " + vcam + " " + vcam.enabled, vcam.gameObject);
    }
    
    public virtual void FailChallenge() { }
    protected virtual void CompleteChallenge() {
        Completed = true;
        CleanupChallenge();
        manager.CompleteChallege(spikeSpline, spike);
    }

    private IEnumerator EnableColliderAfterDelay() {
        yield return new WaitForSeconds(2);
        introduceTrigger.gameObject.SetActive(true);
    }

    public string GetFullName() {
        return "Challenge - " + challengeType + challengeName;
    }

    private void CleanupChallenge() {
        StopAllCoroutines();
        HUD.MessageOverlayCinematic.Clear();
        Player.CanControl = true;
        CameraController.DisableCinemachineCamera(vcam);
        GameManager.SetPlayState(GameManager.GamePlayState.Standard);
        challengeObjects.gameObject.SetActive(false);
        // Reset failure tags
        if (failureObjects != null) {
            for (int i = 0; i < failureObjects.Length; i++) {
                failureObjects[i].tag = failureObjectTags[i];
            }
        }
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
