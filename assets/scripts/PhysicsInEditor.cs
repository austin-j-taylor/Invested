using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PhysicsInEditor : MonoBehaviour
{
    PhysicsScene physicsScene;
    // Update is called once per frame
    void Update() {
        physicsScene.Simulate(Time.fixedDeltaTime);
    }
}
