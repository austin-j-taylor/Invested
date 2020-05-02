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

    private Image circle;

    private Image[] hairs;
    private Image[] fills;
    private Transform hairsHeader;
    

    void Awake() {
        circle = transform.Find("Circle").GetComponent<Image>();

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

        SetManual();
    }

    public void Clear() {
        SetFillPercent(0);
    }

    public void SetFillPercent(float rate) {
        for(int i = 0; i < size; i++) {
            fills[i].fillAmount = rate;
        }
        circle.material.SetFloat("_Fill", rate);
    }

    public void SetCircleRadius(float screenPercentage) {
        // screenPercentage: % of the vertical screen occupied.
        // i.e. if screenPercentage = 25%, then the middle 1/4 of the screen is occupied.
        // The Circle takes up 1080 * 25% * sqrt(2) = 678.82250, which is at 25% when radius = .5.
        float radius = screenPercentage / PlayerPullPushController.maxAreaRadius / 2;

        circle.material.SetFloat("_Radius", radius);
    }
    
    // Sets the crosshairs for the "Manual" control mode
    public void SetManual() {
        circle.gameObject.SetActive(false);
        hairsHeader.gameObject.SetActive(true);
        circle.material.SetFloat("_Radius", 0);
    }
    // Sets the crosshairs for the "Area" control mode
    public void SetArea() {
        circle.gameObject.SetActive(true);
        hairsHeader.gameObject.SetActive(false);
    }
}
