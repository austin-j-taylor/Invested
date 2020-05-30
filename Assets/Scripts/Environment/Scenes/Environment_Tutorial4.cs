﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TextCodes;

public class Environment_Tutorial4 : EnvironmentCinematic {

    EnvironmentalTransitionManager musicManager;

    void Start() {
        Player.VoidHeight = -1000;
        musicManager = GetComponentInChildren<EnvironmentalTransitionManager>();

        Player.CanControl = false;
        Player.CanControlMovement = false;
        Player.PlayerInstance.CoinHand.Pouch.Clear();
        Player.CanControlZinc = false;
        HUD.ControlWheelController.SetLockedState(ControlWheelController.LockedState.LockedToBubble);
        HUD.HelpOverlayController.SetLockedState(HelpOverlayController.LockedState.Locked0);

        // Set cinemachine virtual camera properties
        InitializeCinemachine();
        // Set camera target position to be same as where we left of in tutorial (just a sine function of time)
        vcam.transform.position = new Vector3(Mathf.Cos(Time.unscaledTime * Environment_TitleScreen.speedVert) * Environment_TitleScreen.speedAmp, (1 + Mathf.Sin(Time.unscaledTime * Environment_TitleScreen.speedVert)) * Environment_TitleScreen.speedAmp, Mathf.Sin(Time.unscaledTime * Environment_TitleScreen.speedVert) * Environment_TitleScreen.speedAmp);
        // Make camera look at Player
        vcam.LookAt = Player.PlayerInstance.transform;

        // Handle music
        StartCoroutine(Play_music());

        StartCoroutine(Procedure());
    }

    private IEnumerator Play_music() {
        while (GameManager.AudioManager.SceneTransitionIsPlaying) {
            yield return null;
        }
        musicManager.StartExterior();
    }

    private IEnumerator Procedure() {
        // Skip the intro cutscene if the player is starting elsewhere in the level
        if ((Player.PlayerInstance.transform.position - vcam.transform.position).magnitude < 30) {
            yield return null;
            vcam.enabled = false;
            yield return new WaitForSeconds(2);
        } else {
            Player.PlayerPewter.PewterReserve.IsEnabled = true;
        }
        CameraController.DisableCinemachineCamera(vcam);
        Player.CanControl = true;
        Player.CanControlMovement = true;
        CameraController.Clear();
    }

    protected override IEnumerator Trigger0() {
        while (HUD.ConversationHUDController.IsOpen)
            yield return null;
        Player.CanControlZinc = false;

        HUD.MessageOverlayCinematic.FadeIn(
            KeyZincTime + " to tap " + Zinc + "."
        );
        HUD.MessageOverlayCinematic.FadeOut();
        while (!Player.PlayerZinc.InZincTime) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeIn(
            "The " + ZincBlue("zinc bank") + "drains while tapping zinc, and recharges when not tapping zinc"
        );
    }
    protected override IEnumerator Trigger1() {
        HUD.MessageOverlayCinematic.FadeIn(
            "Opening the " + ControlWheel + " also taps " + Zinc + "."
        );
        while (!Player.PlayerZinc.InZincTime) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOut();
    }
    protected override IEnumerator Trigger2() {
        while (HUD.ConversationHUDController.IsOpen)
            yield return null;
        HUD.MessageOverlayCinematic.FadeIn(
            KeyWalk + " to burn " + Pewter + " to " + OffWhite("anchor") + " yourself and carefully " + OffWhite("balance") + "."
        );

        while (!Player.PlayerPewter.IsAnchoring) {
            yield return null;
        }
        yield return new WaitForSeconds(2);
        HUD.MessageOverlayCinematic.FadeOut();
    }
}