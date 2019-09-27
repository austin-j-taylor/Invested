using UnityEngine;
using System.Collections;

public class LightFlickering : MonoBehaviour {

    private Light flickeringLight;
    private Animator anim;


    // Use this for initialization
    void Awake() {
        flickeringLight = GetComponentInChildren<Light>();
        anim = GetComponentInChildren<Animator>();
    }

    public void FlickerOn() {
        anim.SetTrigger("flickerOn");
    }

    public void On() {
        Debug.Log("turing");
        anim.SetTrigger("on");
    }
}
