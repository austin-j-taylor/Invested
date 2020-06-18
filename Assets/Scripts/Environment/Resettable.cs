using UnityEngine;
using System.Collections;

/**
 * A Resettable is a parent object that, when "activated", will "reset" all of its children.
 * It is "activated" when the Node turns On. Generally, use the connectedNodes field unless this Resettable is a circuit that should reset.
 * When its children are "resetted", different things happen depending on what components they have.
 * In general, reset their position and rotation.
 */
public class Resettable : Node {

    private Transform[] childrenTransforms;
    private Rigidbody[] childrenRigidbodies;

    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;

    public override bool On {
        get {
            return base.On;
        }
        set {
            // If turning from Off to On, Reset.
            if(!base.On && value) {
                Activate();
            }

            base.On = value;
        }
    }

    void Start() {
        childrenTransforms = GetComponentsInChildren<Transform>();
        childrenRigidbodies = GetComponentsInChildren<Rigidbody>();

        initialPositions = new Vector3[childrenTransforms.Length];
        initialRotations = new Quaternion[childrenTransforms.Length];

        for(int i = 0; i < childrenTransforms.Length; i++) {
            initialPositions[i] = childrenTransforms[i].position;
            initialRotations[i] = childrenTransforms[i].rotation;
        }
    }

    public void Activate() {
        Debug.Log("Activating children.");
        for (int i = 0; i < childrenTransforms.Length; i++) {
            childrenTransforms[i].position = initialPositions[i];
            childrenTransforms[i].rotation = initialRotations[i];
        }
        for (int i = 0; i < childrenRigidbodies.Length; i++) {
            childrenRigidbodies[i].velocity = Vector3.zero;
            childrenRigidbodies[i].angularVelocity = Vector3.zero;
        }
    }
}
