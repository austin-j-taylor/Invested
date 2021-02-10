using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Prop", menuName = "ScriptableObjects/PropScriptableObject")]
public class Prop_SO : ScriptableObject {

    // Rig weights
    [SerializeField] private float grab_radius = 0;

    public float Grab_radius => grab_radius;
}