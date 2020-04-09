using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KOLOSSController : MonoBehaviour {

    protected Animator animator;

    public bool ikActive = false;
    public Transform target = null;
    public Transform pole = null;

    void Start() {
        animator = GetComponentInChildren<Animator>();
        target = Player.PlayerInstance.transform;
    }

    //a callback for calculating IK
    void OnAnimatorIK() {
        if (animator) {

            //if the IK is active, set the position and rotation directly to the goal. 
            if (ikActive) {

                // Set the look target position, if one has been assigned
                if (target != null) {
                    animator.SetLookAtWeight(1);
                    animator.SetLookAtPosition(target.position);
                }

                // Set the right hand target position and rotation, if one has been assigned
                if (target != null) {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);
                    //animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, target.position);
                    animator.SetIKHintPosition(AvatarIKHint.RightElbow, pole.position);
                    //animator.SetIKRotation(AvatarIKGoal.RightHand, target.rotation);
                }

            }

            //if the IK is not active, set the position and rotation of the hand and head back to the original position
            else {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                //animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                animator.SetLookAtWeight(0);
            }
        }
    }
}
