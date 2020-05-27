using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PressurePlate : Source {

    [SerializeField]
    private float massThreshold = 10;

    public override bool On {
        set {
            if (value && !On)
                GetComponent<AudioSource>().Play();
            base.On = value;
            GetComponent<MeshRenderer>().material = value ? GameManager.Material_MARLmetal_lit : GameManager.Material_MARLmetal_unlit;
        }
    }

    private Collider presenceTrigger;

    private List<Rigidbody> present;

    private void Start() {
        presenceTrigger = GetComponent<Collider>(); // must be first
        present = new List<Rigidbody>();
    }
    private void Update() {
        if(present.Count > 0) {
            float mass = 0;
            for(int i = 0; i < present.Count; i++) {
                mass += present[i].mass;
            }
            On = mass >= massThreshold;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.attachedRigidbody && !other.isTrigger) {
            present.Add(other.attachedRigidbody);
        }
    }
    private void OnTriggerExit(Collider other) {
        if (other.attachedRigidbody && !other.isTrigger) {
            present.Remove(other.attachedRigidbody);
            if (present.Count == 0)
                On = false;
        }
    }
}
