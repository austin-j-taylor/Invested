using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTrialRing : MonoBehaviour
{
    [HideInInspector]
    public Challenge_TimeTrial trial; // set by the Challenge_TimeTrial
    public bool Passed { get; private set; }

    private Renderer rend;
    private Collider col;

    private void Awake() {
        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();
    }

    public void Clear() {
        Passed = false;
        rend.enabled = true;
    }

    public void Hide() {
        Color color = rend.material.GetColor("_BaseColor");
        color.a = 0;
        rend.material.SetColor("_BaseColor", color);
        col.enabled = false;
    }
    public void Show() {
        Color color = rend.material.GetColor("_BaseColor");
        color.a = 1;
        rend.material.SetColor("_BaseColor", color);
    }

    private void OnTriggerEnter(Collider other) {
        if(Player.IsPlayerTrigger(other)) {
            Passed = true;
            rend.enabled = false;
            col.enabled = false;
            GetComponent<AudioSource>().Play();
        }
    }

}
