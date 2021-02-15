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

    public void OnHit() {
        anim.SetTrigger("OnHit");
    }

    public void Die() {
        anim.SetTrigger("Die");
    }

    #region transitions
    public void State_toIdle() {
        anim.SetBool("PickingUp", false);
        anim.SetBool("Walking", false);
        anim.SetBool("Grabbed", false);
        anim.SetBool("Anchored Pull", false);
        anim.SetBool("Anchored Push", false);
    }
    public void State_toSurprised() {
        anim.SetTrigger("Surprised");
    }
    public void State_toGettingBlock() {
        anim.SetBool("PickingUp", false);
        anim.SetBool("Walking", true);
        anim.SetBool("Grabbed", false);
    }
    public void State_toPickingUp() {
        anim.SetBool("PickingUp", true);
        anim.SetBool("Walking", false);
        anim.SetBool("Grabbed", false);
    }
    public void State_toCaught() {
        anim.SetTrigger("Caught");
    }
    public void State_toMovingToEnemy() {
        anim.SetBool("PickingUp", false);
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
    public void State_toThrown() {

    }
    #endregion
}
