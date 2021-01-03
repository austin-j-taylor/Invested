using UnityEngine;
using System.Collections;

/**
 * A Resettable is a parent object that, when "activated", will "reset" all of its children.
 * It is "activated" when the Node turns On (and resetWhenNodeTurnsOn is true), or when Activate is called.
 * When its children are "resetted", different things happen depending on what components they have.
 * In general, reset their position and rotation. If they're Powered, reset that too
 */
public class Resettable : Node {

    private Transform[] childrenTransforms;
    private Rigidbody[] childrenRigidbodies;
    private Powered[] childrenPowered;

    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;
    private bool[] initialPowereds;

    [SerializeField]
    private bool resetWhenNodeTurnsOn = true;

    public override bool On {
        get {
            return base.On;
        }
        set {
            // If turning from Off to On, Reset.
            if(!base.On && value) {
                if(resetWhenNodeTurnsOn)
                    Activate();
            }

            base.On = value;
        }
    }

    void Start() {
        childrenTransforms = GetComponentsInChildren<Transform>();
        childrenRigidbodies = GetComponentsInChildren<Rigidbody>();
        childrenPowered = GetComponentsInChildren<Powered>();

        initialPositions = new Vector3[childrenTransforms.Length];
        initialRotations = new Quaternion[childrenTransforms.Length];
        initialPowereds = new bool[childrenPowered.Length];

        for(int i = 0; i < childrenTransforms.Length; i++) {
            initialPositions[i] = childrenTransforms[i].position;
            initialRotations[i] = childrenTransforms[i].rotation;
        }
        for (int i = 0; i < childrenPowered.Length; i++) {
            initialPowereds[i] = childrenPowered[i].On;
        }
    }

    public void Activate() {
        for (int i = 0; i < childrenTransforms.Length; i++) {
            childrenTransforms[i].position = initialPositions[i];
            childrenTransforms[i].rotation = initialRotations[i];
        }
        for (int i = 0; i < childrenRigidbodies.Length; i++) {
            childrenRigidbodies[i].velocity = Vector3.zero;
            childrenRigidbodies[i].angularVelocity = Vector3.zero;
        }
        for (int i = 0; i < childrenPowered.Length; i++) {
            Debug.Log(initialPowereds[i]);
            childrenPowered[i].On = initialPowereds[i];
        }
    }
}
