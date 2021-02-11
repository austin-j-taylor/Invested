using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Kog : Actor {

    #region constants
    #endregion

    public static Kog KogInstance { get; private set; }

    // State machine for Kog
    public enum State { Idle, Resting, Reaching, Throwing, Meditating };

    public static KogPullPushController IronSteel { get; private set; }
    public static KogAudioController AudioController { get; private set; }
    public static KogAnimation KogAnimationController { get; private set; }
    public static KogMovementController MovementController { get; private set; }
    public static KogHandController HandController { get; private set; }

    protected override void Awake() {
        IronSteel = GetComponent<KogPullPushController>();
        AudioController = GetComponentInChildren<KogAudioController>();
        KogAnimationController = GetComponentInChildren<KogAnimation>();
        MovementController = GetComponent<KogMovementController>();
        HandController = GetComponentInChildren<KogHandController>();

        KogInstance = this;
        Type = ActorType.Kog;
        RespawnHeightOffset = -0.26f;
        CameraScale = 2f;
        CameraOffsetThirdPerson = 2.5f;
        CameraOffsetFirstPerson = 3.5f;

        base.Awake();

        gameObject.SetActive(false);

        // Invoked by the Player to prevent race condition
        //SceneManager.sceneLoaded += ClearKogAfterSceneChange;
        //SceneManager.sceneUnloaded += ClearKogBeforeSceneChange;
    }

    public void ClearKogBeforeSceneChange(Scene scene) {
        AudioController.Clear();
        MovementController.Clear();
        HandController.Clear();
    }
    /// <summary>
    /// Reset certain values AFTER the player enters a new scene
    /// </summary>
    /// <param name="scene">the scene that will be entered</param>
    /// <param name="mode">the sceme loading mode</param>
    public void ClearKogAfterSceneChange(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Single) { // Not loading all of the scenes, as it does at startup
            KogAnimationController.Clear();
        }
    }
}
