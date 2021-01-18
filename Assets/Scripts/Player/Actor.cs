using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Child interface for the entities controllable by the player, Prima and Kog.
/// </summary>
public class Actor : MonoBehaviour {
    public enum ActorType { Prima, Kog };

    // Assigned in child's Awake()
    protected ActorType type;
    public float RespawnHeightOffset { get; protected set; }

    public PrimaPullPushController ActorIronSteel { get; private set; }
    public VoiceBeeper ActorVoiceBeeper { get; set; }


    protected virtual void Awake() {
        ActorIronSteel = GetComponentInChildren<PrimaPullPushController>();
        ActorVoiceBeeper = GetComponentInChildren<VoiceBeeper>();


        SceneManager.sceneLoaded += ClearActorAfterSceneChange;
    }
    public virtual void RespawnClear() {
        ActorIronSteel.Clear();
    }

    /// <summary>
    /// Reset certain values AFTER the player enters a new scene
    /// </summary>
    /// <param name="scene">the scene that will be entered</param>
    /// <param name="mode">the sceme loading mode</param>
    private void ClearActorAfterSceneChange(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Single) { // Not loading all of the scenes, as it does at startup
            ActorIronSteel.Clear();
        }
    }
}
