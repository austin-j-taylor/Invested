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

    public KogAnimation KogAnimationController { get; private set; }

    public KogMovementController MovementController { get; private set; }

    protected override void Awake() {
        KogInstance = this;
        Type = ActorType.Kog;
        RespawnHeightOffset = -0.26f;
        CameraScale = 2f;
        CameraOffsetThirdPerson = 2.5f;
        CameraOffsetFirstPerson = 3.5f;

        base.Awake();

        KogAnimationController = GetComponentInChildren<KogAnimation>();
        MovementController = GetComponent<KogMovementController>();

        gameObject.SetActive(false);

        SceneManager.sceneLoaded += ClearKogAfterSceneChange;
        SceneManager.sceneUnloaded += ClearKogBeforeSceneChange;
    }

    public void ClearKogBeforeSceneChange(Scene scene) {
        MovementController.Clear();
        KogAnimationController.Clear();
    }

    /// <summary>
    /// Reset certain values AFTER the player enters a new scene
    /// </summary>
    /// <param name="scene">the scene that will be entered</param>
    /// <param name="mode">the sceme loading mode</param>
    private void ClearKogAfterSceneChange(Scene scene, LoadSceneMode mode) {
    }
}
