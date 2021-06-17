﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Controls the HUD element that overlays a hostile entity.
/// Includes the reticle element that appears over a target when aiming at it.
/// </summary>
public class TargetEntityController : MonoBehaviour {

    private const int reticle_offset = 42;
    private const int bar_offset = 50;
    private const int num_bars = 10;

    private enum ReticleControlState { None, Aiming, LockedOn };

    [SerializeField]
    private TargetBar templateBar = null;

    private Animator anim;
    private ReticleControlState state;

    private Image reticle;
    private Bounds lastMarkedBounds;
    private TargetBar[] bars;

    // Use this for initialization
    void Awake() {
        anim = GetComponent<Animator>();
        state = ReticleControlState.None;
        reticle = transform.Find("reticle").GetComponent<Image>();

        bars = new TargetBar[num_bars];
        for(int i = 0; i < num_bars; i++) {
            bars[i] = Instantiate(templateBar, transform, false);
            bars[i].Close();
        }
    }
    public void Clear() {
        State_toNone();
        foreach (TargetBar bar in bars)
            bar.Clear();
    }

    private void LateUpdate() {
        // RETICLE
        // Transition
        switch (state) {
            case ReticleControlState.None:
                if (Kog.HandController.MarkedEntity != null)
                    State_toAiming();
                break;
            case ReticleControlState.Aiming:
                if (Kog.HandController.MarkedEntity == null)
                    State_toNone();
                else if(Kog.IronSteel.State == KogPullPushController.PullpushMode.Throwing)
                    State_toLockedOn();
                break;
            case ReticleControlState.LockedOn:
                if (Kog.HandController.MarkedEntity == null)
                    State_toNone();
                if (Kog.IronSteel.State != KogPullPushController.PullpushMode.Throwing && Kog.HandController.State != KogHandController.GrabState.LockedOn)
                    State_toNone();
                break;
        }
        // Action
        switch (state) {
            case ReticleControlState.None:
                break;
            case ReticleControlState.Aiming:
            case ReticleControlState.LockedOn:
                // Move the reticle to align with the target's transform
                lastMarkedBounds = Kog.HandController.MarkedEntity.BoundingBox.bounds;
                anim.SetFloat("Locking On Time", Time.timeScale);
                break;
        }
        if (reticle.color.a > 0.001f) {
            Vector3 v_dx = CameraController.ActiveCamera.transform.right * lastMarkedBounds.size.x / 2f;
            Vector3 v_dy = CameraController.ActiveCamera.transform.up * lastMarkedBounds.size.y / 2f;
            Vector3 screen_center = CameraController.ActiveCamera.WorldToScreenPoint(lastMarkedBounds.center);
            Vector3 screen_dx = CameraController.ActiveCamera.WorldToScreenPoint(lastMarkedBounds.center + v_dx) - screen_center;
            Vector3 screen_dy = CameraController.ActiveCamera.WorldToScreenPoint(lastMarkedBounds.center + v_dy) - screen_center;
            reticle.rectTransform.offsetMin = new Vector2(-screen_dx.x - reticle_offset, -screen_dy.y - reticle_offset);
            reticle.rectTransform.offsetMax = new Vector2(screen_dx.x + reticle_offset, screen_dy.y + reticle_offset);
            reticle.rectTransform.position = screen_center;
        }

        // BARS
        // Go over entities [in scene].
        int i;
        for(i = 0; i < GameManager.EntitiesInScene.Count && i < num_bars; i++) {
            Collider boundingBox = GameManager.EntitiesInScene[i].BoundingBox;
            if(boundingBox != null) {
                Vector3 v_y = boundingBox.bounds.center + CameraController.ActiveCamera.transform.up * boundingBox.bounds.size.y / 2f;
                Vector3 screen_y = CameraController.ActiveCamera.WorldToScreenPoint(v_y) + new Vector3(0, bar_offset, 0);
                if (screen_y.z < 0) {
                    // off screen
                    bars[i].transform.position = new Vector3(-100000, -100000, 0);
                } else {
                    if (GameManager.EntitiesInScene[i].Hostile) {
                        bars[i].Open(GameManager.EntitiesInScene[i]);
                    } else {
                        bars[i].Close();
                    }
                    bars[i].transform.position = screen_y;
                }
            }
        }
        while(i < num_bars) {
            bars[i].Close();
            i++;
        }
    }

    private void State_toNone() {
        anim.SetBool("Aiming", false);
        anim.SetBool("Locked On", false);
        state = ReticleControlState.None;
    }
    private void State_toAiming() {
        anim.SetBool("Aiming", true);
        anim.SetBool("Locked On", false);
        state = ReticleControlState.Aiming;
    }
    private void State_toLockedOn() {
        anim.SetBool("Locked On", true);
        state = ReticleControlState.LockedOn;
    }
}
