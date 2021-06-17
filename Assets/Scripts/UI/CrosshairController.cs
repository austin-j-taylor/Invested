﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the HUD element for the crosshairs at the center of the screen
/// </summary>
public class CrosshairController : MonoBehaviour {

    // positions in the hairs array
    private const int top = 0, left = 1, bottom = 2, right = 3, size = 4;
    private const float alphaHigh = .75f, alphaLow = .25f, alphaMedium = 0.5f;

    private enum CrosshairState { None, Manual, Area, Coinshot, StopBurning, Kog };

    private CrosshairState state;
    private Image circle, kogCircle;
    private Color blueColor, hairsColor, goldColor = HUD.goldColor;
    private Image[] hairs;
    private Image[] fills;
    private Transform hairsHeader;

    void Awake() {
        circle = transform.Find("Circle").GetComponent<Image>();
        kogCircle = transform.Find("KogCircle").GetComponent<Image>();

        hairsHeader = transform.Find("Hairs");

        hairs = new Image[size];
        fills = new Image[size];
        hairs[top] = hairsHeader.GetChild(top).GetComponent<Image>();
        hairs[left] = hairsHeader.GetChild(left).GetComponent<Image>();
        hairs[bottom] = hairsHeader.GetChild(bottom).GetComponent<Image>();
        hairs[right] = hairsHeader.GetChild(right).GetComponent<Image>();
        // assign fills...
        fills[top] = hairs[top].transform.GetChild(0).GetComponent<Image>();
        fills[left] = hairs[left].transform.GetChild(0).GetComponent<Image>();
        fills[bottom] = hairs[bottom].transform.GetChild(0).GetComponent<Image>();
        fills[right] = hairs[right].transform.GetChild(0).GetComponent<Image>();

        // Set circle material to be a copy of its template so we don't overwrite the actual material's values
        circle.material = Instantiate(circle.material);

        blueColor = fills[top].color;
        hairsColor = hairs[top].color;

        state = CrosshairState.None;
        Hide();
    }

    private void LateUpdate() {
        // Change crosshair to reflect current actor's control mode
        switch (Player.CurrentActor.Type) {
            case Actor.ActorType.Prima:
                switch (state) {
                    case CrosshairState.None:
                        State_toManual();
                        break;
                    case CrosshairState.Manual:
                        if (Player.CurrentActor.ActorIronSteel.IsBurning) {
                            switch (Player.CurrentActor.ActorIronSteel.Mode) {
                                case ActorPullPushController.ControlMode.Area:
                                    State_toArea();
                                    break;
                                case ActorPullPushController.ControlMode.Coinshot:
                                    State_toCoinshot();
                                    break;
                            }
                        }
                        break;
                    case CrosshairState.Area:
                        if (Player.CurrentActor.ActorIronSteel.IsBurning) {
                            switch (Player.CurrentActor.ActorIronSteel.Mode) {
                                case ActorPullPushController.ControlMode.Manual:
                                    State_toManual();
                                    break;
                                case ActorPullPushController.ControlMode.Coinshot:
                                    State_toCoinshot();
                                    break;
                            }
                        } else {
                            State_toStopBurning();
                        }
                        break;
                    case CrosshairState.Coinshot:
                        if (Player.CurrentActor.ActorIronSteel.IsBurning) {
                            switch (Player.CurrentActor.ActorIronSteel.Mode) {
                                case ActorPullPushController.ControlMode.Manual:
                                    State_toManual();
                                    break;
                                case ActorPullPushController.ControlMode.Area:
                                    State_toArea();
                                    break;
                            }
                        } else {
                            State_toStopBurning();
                        }
                        break;
                    case CrosshairState.StopBurning:
                        State_toManual();
                        break;
                    case CrosshairState.Kog:
                        State_toManual();
                        break;
                }
                break;
            case Actor.ActorType.Kog:
                switch (state) {
                    case CrosshairState.None:
                        if(Kog.IronSteel.State == KogPullPushController.PullpushMode.Burning || Kog.IronSteel.State == KogPullPushController.PullpushMode.Active)
                            State_toKog();
                        break;
                    case CrosshairState.Manual:
                        State_toKog();
                        break;
                    case CrosshairState.Area:
                        State_toKog();
                        break;
                    case CrosshairState.Coinshot:
                        State_toKog();
                        break;
                    case CrosshairState.StopBurning:
                        State_toKog();
                        break;
                    case CrosshairState.Kog:
                        if (Kog.IronSteel.State != KogPullPushController.PullpushMode.Burning && Kog.IronSteel.State != KogPullPushController.PullpushMode.Active)
                            State_toNone();
                        break;
                }
                break;
        }

        // Actions
        switch (state) {
            case CrosshairState.None:
                break;
            case CrosshairState.Manual:
                SetFillPercent(Mathf.Max(Player.CurrentActor.ActorIronSteel.IronBurnPercentageTarget, Player.CurrentActor.ActorIronSteel.SteelBurnPercentageTarget));
                break;
            case CrosshairState.Area:
                SetFillPercent(Mathf.Max(Player.CurrentActor.ActorIronSteel.IronBurnPercentageTarget, Player.CurrentActor.ActorIronSteel.SteelBurnPercentageTarget));
                SetCircleRadius(Player.CurrentActor.ActorIronSteel.AreaRadiusLerp);
                break;
            case CrosshairState.Coinshot:
                SetFillPercent(Mathf.Max(Player.CurrentActor.ActorIronSteel.IronBurnPercentageTarget, Player.CurrentActor.ActorIronSteel.SteelBurnPercentageTarget));
                break;
            case CrosshairState.Kog:
                break;
        }
    }

    #region transitions
    private void State_toNone() {
        Clear();
        Hide();

        state = CrosshairState.None;
    }
    private void State_toManual() {
        Show();
        //circle.gameObject.SetActive(false);
        blueColor.a = alphaHigh;
        hairsHeader.gameObject.SetActive(true);
        for (int i = 0; i < size; i++) {
            fills[i].color = blueColor;
        }
        StartCoroutine(LerpToCircleRatio(1));

        state = CrosshairState.Manual;
    }
    private void State_toArea() {
        Show();
        //circle.gameObject.SetActive(true);
        hairsHeader.gameObject.SetActive(false);
        StartCoroutine(LerpToCircleRatio(.66666f));
        blueColor.a = alphaHigh;
        circle.material.SetColor("_Color", blueColor);

        state = CrosshairState.Area;
    }
    private void State_toStopBurning() {
        Clear();
        state = CrosshairState.StopBurning;
    }
    //private void State_toBubble() {
    //    Show();
    //    //circle.gameObject.SetActive(true);
    //    hairsHeader.gameObject.SetActive(false);
    //    blueColor.a = alphaLow;
    //    circle.material.SetColor("_Color", blueColor);
    //    StartCoroutine(LerpToCircleRatio(0));
    //}
    private void State_toCoinshot() {
        Show();
        //circle.gameObject.SetActive(false);
        hairsHeader.gameObject.SetActive(true);
        for (int i = 0; i < size; i++) {
            fills[i].color = goldColor;
        }
        StartCoroutine(LerpToCircleRatio(1));

        state = CrosshairState.Coinshot;
    }
    private void State_toKog() {
        Clear();
        kogCircle.enabled = true;

        state = CrosshairState.Kog;
    }
    #endregion

    public void Clear() {
        SetFillPercent(0);
        circle.material.SetFloat("_RatioLow", 1);
        kogCircle.enabled = false;
    }

    private void Hide() {
        hairsColor.a = 0;
        for (int i = 0; i < size; i++) {
            hairs[i].color = hairsColor;
        }
    }

    private void Show() {
        kogCircle.enabled = false;
        StopAllCoroutines();
        hairsColor.a = alphaMedium;
        for (int i = 0; i < size; i++) {
            hairs[i].color = hairsColor;
        }
    }

    private void SetFillPercent(float rate) {
        for (int i = 0; i < size; i++) {
            fills[i].fillAmount = rate;
        }
        circle.material.SetFloat("_Fill", rate);
    }

    private void SetCircleRadius(float screenPercentage) {
        // screenPercentage: % of the vertical screen occupied.
        // i.e. if screenPercentage = 25%, then the middle 1/4 of the screen is occupied.
        // The Circle takes up 1080 * 25% * sqrt(2) = 678.82250, which is at 25% when radius = .5.
        float radius = screenPercentage / PrimaPullPushController.maxAreaRadius / 2;

        circle.material.SetFloat("_Radius", radius);
    }

    private IEnumerator LerpToCircleRatio(float targetRatio) {
        float count = 0, ratio = circle.material.GetFloat("_RatioLow");
        while (count < 1) {
            count += Time.unscaledDeltaTime * 4;
            circle.material.SetFloat("_RatioLow", Mathf.Lerp(ratio, targetRatio, count));
            yield return null;
        }
        circle.material.SetFloat("_RatioLow", targetRatio);
    }
}
