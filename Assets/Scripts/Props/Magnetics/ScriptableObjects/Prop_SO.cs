using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Prop", menuName = "ScriptableObjects/PropScriptableObject")]
public class Prop_SO : ScriptableObject {

    // Other
    [SerializeField] private bool getCollidersFromRigidbody = false;

    // Rig weights
    [SerializeField] private float grab_pos_y = 0;
    [SerializeField] private float grab_pos_z = 0;
    [SerializeField] private float grab_rotation_z = 0;

    public bool GetCollidersFromRigidbody => getCollidersFromRigidbody;
    public float Grab_pos_y => grab_pos_y;
    public float Grab_pos_z => grab_pos_z;
    public float Grab_rotation_z => grab_rotation_z;
}