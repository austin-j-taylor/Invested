using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Kog;

// Scripted animation for Kog
public class KogAnimation : MonoBehaviour {

    private const float zeroThresholdSqr = 0.05f * 0.05f;

    [SerializeField]
    private Transform target = null;
    [SerializeField]
    private Transform hand = null;
    public Transform pole = null;

    public State state;

    private Animator anim;

    void Start() {
        anim = GetComponentInChildren<Animator>();
        target = Player.PlayerInstance.transform;
        //state = State.Resting;

        switch (state) {
            case State.Meditating:
                anim.Play("Armature|Meditating");
                break;
            case State.Idle:
                anim.Play("Armature|I-Pose");
                break;
        }
    }

    void Update() {
        switch (state) {
            case State.Reaching:
                // Transition: If the Sphere is close to the hand
                if ((Player.PlayerInstance.transform.position - hand.position).sqrMagnitude < zeroThresholdSqr) {
                    state = State.Throwing;
                    anim.SetBool("Throwing", true);
                }
                break;
            case State.Throwing:
                // For no...
                if (Keybinds.JumpDown()) {
                    state = State.Reaching;
                    anim.SetBool("Throwing", false);
                }
                break;
        }
    }

    //a callback for calculating IK
    void OnAnimatorIK() {

        if (anim) {
            switch (state) {
                case State.Resting:
                    anim.SetLookAtWeight(1);
                    anim.SetLookAtPosition(target.position);
                    break;
                case State.Reaching:
                    //if the IK is active, set the position and rotation directly to the goal. 
                    // Set the look target position, if one has been assigned
                    if (target != null) {
                        anim.SetLookAtWeight(1);
                        anim.SetLookAtPosition(target.position);
                    }

                    // Set the right hand target position and rotation, if one has been assigned
                    if (target != null) {
                        // set pole position to be to the right of the KOLOSS

                        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                        //anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                        anim.SetIKPosition(AvatarIKGoal.RightHand, target.position);
                        if (pole) {
                            pole.position = -Vector3.Cross((target.position - transform.position).normalized, Vector3.up) + target.position;
                            anim.SetIKHintPosition(AvatarIKHint.RightElbow, pole.position);
                            anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);
                        }
                        //anim.SetIKRotation(AvatarIKGoal.RightHand, target.rotation);
                    }

                    break;
                case State.Throwing:
                    //if the IK is not active, set the position and rotation of the hand and head back to the original position
                    anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                    anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                    anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);
                    anim.SetLookAtWeight(0);
                    break;
            }
        }
    }
}