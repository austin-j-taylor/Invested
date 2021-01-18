using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Kog : Actor {

    #region constants
    #endregion

    public static Kog KogInstance { get; private set; }

    // State machine for Kog
    public enum State { Resting, Reaching, Throwing, Meditating };

    private KogAnimation kogAnimation;

    public KogMovementController MovementController { get; private set; }

    protected override void Awake() {
        KogInstance = this;
        type = ActorType.Kog;
        RespawnHeightOffset = 5;

        base.Awake();

        kogAnimation = GetComponentInChildren<KogAnimation>();
        MovementController = GetComponent<KogMovementController>();

        SceneManager.sceneLoaded += ClearKogAfterSceneChange;
        SceneManager.sceneUnloaded += ClearKogBeforeSceneChange;
    }

    public void ClearKogBeforeSceneChange(Scene scene) {
        MovementController.Clear();
    }

    /// <summary>
    /// Reset certain values AFTER the player enters a new scene
    /// </summary>
    /// <param name="scene">the scene that will be entered</param>
    /// <param name="mode">the sceme loading mode</param>
    private void ClearKogAfterSceneChange(Scene scene, LoadSceneMode mode) {

    }
}
