using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

[System.Serializable]
public class Wire : Powered {

    [SerializeField]
    private GameObject objectWire = null;
    [SerializeField]
    public Vector3[] points;
    [SerializeField]
    public Renderer[] rends;

    public int PointCount { get => points.Length; }

    public void Reset() {
        points = new Vector3[] {
            new Vector3(0, 0f, 1f),
        };
        rends = new Renderer[0];
        for(int i = 0; i < transform.childCount; i++) {
            Transform child = transform.GetChild(i);
            if (child.name.Contains("Wire"))
                DestroyImmediate(child.gameObject);
        }
    }
    public void AddWire() {
        Vector3 point = points[points.Length - 1];
        Array.Resize(ref points, points.Length + 1);
        Array.Resize(ref rends, rends.Length + 1);
        Vector3 direction = transform.forward;
        if (points.Length > 3)
            direction = (points[points.Length - 2] - points[points.Length - 3]).normalized;
        point += direction;
        points[points.Length - 1] = point;

        // Instantiate a wire object
        rends[rends.Length-1] = Instantiate(objectWire, transform.TransformPoint(points[points.Length - 1]), Quaternion.LookRotation(direction), transform).GetComponent<Renderer>();
        UpdateWire(rends.Length - 1);
    }
    public void RemoveWire() {
        if (PointCount > 0) {
            int i = points.Length - 1;
            Array.Resize(ref points, i);
            i--;
            if (i < transform.childCount) {
                Transform child = transform.GetChild(i);
                DestroyImmediate(child.gameObject);
                Debug.Log("destroyed child " + i);
            } else {
                Debug.Log("too big child " + i);

            }
            Array.Resize(ref rends, i);
        }
    }
    public Vector3 GetPoint(int index) { return points[index]; }
    public void MovePoint(int index, Vector3 position) {
        points[index] = position;
        if(index > 0)
            UpdateWire(index - 1);
        if(index < points.Length - 1)
            UpdateWire(index);
    }
    private void UpdateWire(int i) {
        // position: midpoint between last two points
        // rotation: angle between last two points
        // scale in the Z: distance between last two points
        int last = i, next = i + 1;
        Vector3 direction = (points[next] - points[last]);
        float distance = (points[next] - points[last]).magnitude;

        rends[i].transform.localPosition = points[last] + (direction) /2;
        Vector3 scale = rends[i].transform.localScale;
        scale.z = distance;
        rends[i].transform.localScale = scale;
        rends[i].transform.rotation = Quaternion.LookRotation(direction);
    }

    public override bool On {
        set {
            base.On = value;
            for(int i = 0; i < rends.Length; i++) {
                rends[i].material = value ? GameManager.Material_MARLmetal_lit : GameManager.Material_MARLmetal_unlit;
            }
        }
    }
}
