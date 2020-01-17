using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PhysicsInEditor : MonoBehaviour
{
    // Update is called once per frame
    PhysicsScene sce;
    void OnRenderObject() {
        //DeleteOutOfRange();
        //sce.Simulate(Time.fixedDeltaTime);
    }

    private void DeleteOutOfRange() {
        GameObject[] objs = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (GameObject go in objs) {
            if (go.transform.position.magnitude > 500 && go.GetComponent<Magnetic>()) {
                Debug.Log(go.transform.position, go);
                DestroyImmediate(go);
            }
        }
    }
}
