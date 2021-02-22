using UnityEngine;
using System.Collections;

public class TargetBarIcon : MonoBehaviour {

    public Animator anim;

    private void Awake() {
        anim = GetComponent<Animator>();
    }
}
