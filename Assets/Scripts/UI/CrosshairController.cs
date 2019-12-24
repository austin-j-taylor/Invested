using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * Controls the crosshairs at the center of the screen
 */
public class CrosshairController : MonoBehaviour
{
    // positions in the hairs array
    private const int top = 0, left = 1, bottom = 2, right = 3, size = 4;

    private Image[] hairs;
    private Image[] fills;

    void Awake() {
        Transform header = transform.Find("Hairs");

        hairs = new Image[size];
        fills = new Image[size];
        hairs[top] = header.GetChild(top).GetComponent<Image>();
        hairs[left] = header.GetChild(left).GetComponent<Image>();
        hairs[bottom] = header.GetChild(bottom).GetComponent<Image>();
        hairs[right] = header.GetChild(right).GetComponent<Image>();
        // assign fills...
        fills[top] = hairs[top].transform.GetChild(0).GetComponent<Image>();
        fills[left] = hairs[left].transform.GetChild(0).GetComponent<Image>();
        fills[bottom] = hairs[bottom].transform.GetChild(0).GetComponent<Image>();
        fills[right] = hairs[right].transform.GetChild(0).GetComponent<Image>();
    }

    public void Clear() {
        SetFillPercent(0);
    }

    public void SetFillPercent(float rate) {
        for(int i = 0; i < size; i++) {
            fills[i].fillAmount = rate;
        }
    }
}
