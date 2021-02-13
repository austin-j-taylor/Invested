using UnityEngine;
using System.Collections;
using static Waddler;

public class WaddlerAnimation : MonoBehaviour {

    private Animator anim;
    private Waddler waddler;

    private void Awake() {
        anim = GetComponentInChildren<Animator>();
        waddler = GetComponent<Waddler>();
    }

    //public void LateUpdate() {
    //    // Actions
    //    switch (waddler.State) {
    //        case WaddlerState.Idle:
    //            break;
    //        case WaddlerState.Suprised:
    //            break;
    //        case WaddlerState.GettingBlock:
    //            break;
    //        case WaddlerState.PickingUp:
    //            break;
    //        case WaddlerState.AnchoredPull:
    //            break;
    //        case WaddlerState.AnchoredPush:
    //            break;
    //        case WaddlerState.MovingToEnemy:
    //            break;
    //        case WaddlerState.Throwing:
    //            break;
    //    }
    //}

    public void State_toIdle() {
        anim.SetBool("Walking", false);
        anim.SetBool("Grabbed", false);

    }
    public void State_toSurprised() {
        anim.SetTrigger("Surprised");
    }
    public void State_toGettingBlock() {
        anim.SetBool("Walking", true);
        anim.SetBool("Grabbed", false);
    }
    public void State_toPickingUp() {
        anim.SetTrigger("PickingUp");
        anim.SetBool("Walking", false);
        anim.SetBool("Grabbed", false);
    }
    public void State_toMovingToEnemy() {
        anim.SetTrigger("Caught");
        anim.SetBool("Walking", true);
        anim.SetBool("Grabbed", true);
        anim.SetBool("Anchored Pull", false);
        anim.SetBool("Anchored Push", false);
    }
    public void State_toAnchoredPull() {
        anim.SetBool("Anchored Pull", true);
    }
    public void State_toAnchoredPush() {
        anim.SetBool("Anchored Push", true);
    }
    public void State_toThrowing() {
        anim.SetTrigger("Throw");
        anim.SetBool("Walking", false);
        anim.SetBool("Grabbed", true);
    }
}
