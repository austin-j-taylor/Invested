using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KOLOSSController : MonoBehaviour {

    private const float zeroThresholdSqr = 0.05f * 0.05f;

    // State for the KOLOSS
    public enum State { Resting, Reaching, Throwing };
    public State state;

    protected Animator animator;

    [SerializeField]
    private Transform target = null;
    [SerializeField]
    private Transform hand = null;
    //public Transform pole = null;

    void Start() {
        animator = GetComponentInChildren<Animator>();
        target = Player.PlayerInstance.transform;
        state = State.Resting;
    }

    void Update() {
        switch (state) {
            case State.Reaching:
                // Transition: If the Sphere is close to the hand
                if ((Player.PlayerInstance.transform.position - hand.position).sqrMagnitude < zeroThresholdSqr) {
                    state = State.Throwing;
                    animator.SetBool("Throwing", true);
                }
                break;
            case State.Throwing:
                // For no...
                if(Keybinds.JumpDown()) {
                    state = State.Reaching;
                    animator.SetBool("Throwing", false);
                }
                break;
        }

    }

    //a callback for calculating IK
    void OnAnimatorIK() {

        if (animator) {
            switch(state) {
                case State.Resting:
                    animator.SetLookAtWeight(1);
                    animator.SetLookAtPosition(target.position);
                    break;
                case State.Reaching:
                    //if the IK is active, set the position and rotation directly to the goal. 
                    // Set the look target position, if one has been assigned
                    if (target != null) {
                        animator.SetLookAtWeight(1);
                        animator.SetLookAtPosition(target.position);
                    }

                    // Set the right hand target position and rotation, if one has been assigned
                    if (target != null) {
                        // set pole position to be to the right of the KOLOSS

                        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                        animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);
                        //animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                        animator.SetIKPosition(AvatarIKGoal.RightHand, target.position);
                        //if (pole) {
                        //    pole.position = Vector3.Cross((target.position - transform.position).normalized, Vector3.up) + target.position;
                        //    animator.SetIKHintPosition(AvatarIKHint.RightElbow, pole.position);
                        //}
                        //animator.SetIKRotation(AvatarIKGoal.RightHand, target.rotation);
                    }

                    break;
                case State.Throwing:
                    //if the IK is not active, set the position and rotation of the hand and head back to the original position
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                    animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);
                    animator.SetLookAtWeight(0);
                    break;
            }
        }
    }
}
