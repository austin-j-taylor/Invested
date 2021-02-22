﻿using UnityEngine;
using System.Collections;

/// <summary>
/// A single instance of a health bar-like HUD element that follows an entity,
/// governed by TargetEntityController.
/// </summary>
public class TargetBar : MonoBehaviour {

    private const int maxPossibleHealth = 5;

    // The entity that this bar is bound to
    private Entity currentEntity = null;

    private TargetBarIcon[] icons;

    private void Awake() {
        icons = GetComponentsInChildren<TargetBarIcon>();
    }

    public void LateUpdate() {
        // Set icons to match health of entity
        for (int i = 0; i < currentEntity.MaxHealth && i < maxPossibleHealth; i++) {
            icons[i].anim.SetBool("Hit", currentEntity.Health <= i);
        }
    }
    // Open the bar above the given entity
    public void Open(Entity newEntity) {
        gameObject.SetActive(true);
        if (newEntity != currentEntity) {
            // New entity. This bar should show the "opening" animation. Set the health counter icons.
            currentEntity = newEntity;
            StartCoroutine(OpenIcons());

        }
    }
    public void Close() {
        currentEntity = null;
        gameObject.SetActive(false);
        for (int i = 0; i < maxPossibleHealth; i++) {
            icons[i].anim.SetBool("Visible", false);
            icons[i].anim.SetBool("Hit", false);
        }
    }

    private IEnumerator OpenIcons() {
        for(int i = 0; i < currentEntity.MaxHealth && i < maxPossibleHealth; i++) {
            icons[i].anim.SetBool("Visible", true);
            icons[i].anim.SetBool("Hit", false);
            yield return new WaitForSeconds(0.5f);
        }
    }
}
