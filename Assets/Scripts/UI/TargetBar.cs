using UnityEngine;
using System.Collections;

/// <summary>
/// A single instance of a health bar-like HUD element that follows an entity,
/// governed by TargetEntityController.
/// </summary>
public class TargetBar : MonoBehaviour {

    public void Open() {
        gameObject.SetActive(true);
    }
    public void Close() {
        gameObject.SetActive(false);
    }
}
