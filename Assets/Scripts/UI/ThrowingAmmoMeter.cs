﻿using UnityEngine;
using UnityEngine.UI;

/*
 * HUD element that shows how many coins the player has.
 */
public class ThrowingAmmoMeter : MonoBehaviour {

    private const float timeToFade = 3;

    private Animator anim;
    private Text coinCountText;
    private float timeLastChanged;
    private int lastCount;

    private void Awake() {
        anim = GetComponent<Animator>();
        coinCountText = GetComponentInChildren<Text>();
    }

    private void LateUpdate() {

        if (lastCount != Player.PlayerInstance.CoinHand.Pouch.Count) {
            timeLastChanged = Time.time;
        }

        anim.SetBool("IsVisible", Time.time - timeLastChanged < timeToFade);

        coinCountText.text = Player.PlayerInstance.CoinHand.Pouch.Count.ToString();

        lastCount = Player.PlayerInstance.CoinHand.Pouch.Count;
    }

    public void Clear() {
        timeLastChanged = -100;
        lastCount = 0;
        anim.SetBool("IsVisible", false);
        anim.Play("MetalReserve_Invisible", anim.GetLayerIndex("Visibility"));
    }
}
