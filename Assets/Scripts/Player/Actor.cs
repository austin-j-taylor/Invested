﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Child interface for the entities controllable by the player, Prima and Kog.
/// </summary>
public class Actor : MonoBehaviour {
    public enum ActorType { Prima, Kog };

    // Assigned in child's Awake()
    public ActorType Type { get; protected set; }
    public float RespawnHeightOffset { get; protected set; }
    public float CameraScale { get; protected set; }
    public float CameraOffsetThirdPerson { get; protected set; }
    public float CameraOffsetFirstPerson { get; protected set; }

    public Rigidbody Rb => ActorIronSteel.rb;
    public ActorPullPushController ActorIronSteel { get; private set; }
    public ActorTransparencyController Transparancy { get; set; }
    public VoiceBeeper ActorVoiceBeeper { get; set; }

    protected virtual void Awake() {
        ActorIronSteel = GetComponentInChildren<ActorPullPushController>();
        ActorVoiceBeeper = GetComponentInChildren<VoiceBeeper>();
        Transparancy = GetComponentInChildren<ActorTransparencyController>();

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
