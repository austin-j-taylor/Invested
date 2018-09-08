using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarmonySphere : MonoBehaviour {

    private const float distanceThreshold = .001f;
    private const float forceConstantFar = 50f;

    private AllomanticIronSteel player;
    private Rigidbody rb;
    private Transform harmonySphere;
    private Transform inner;

    private void Awake() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<AllomanticIronSteel>();
        rb = GetComponentInChildren<Rigidbody>();
        harmonySphere = rb.GetComponent<Transform>();
        inner = transform.GetChild(0);
    }
    
    private void FixedUpdate() {
        if (!rb.IsSleeping()) {
            Vector3 distancetoPlayer = harmonySphere.position - CameraController.ActiveCamera.transform.position;// player.transform.position;

            float angle = 180 + Mathf.Atan2(distancetoPlayer.x, distancetoPlayer.z) * Mathf.Rad2Deg;
            Vector3 newRotation = Vector3.zero;
            //angle = Mathf.LerpAngle(inner.eulerAngles.y, angle, Time.deltaTime * 10f);

            newRotation.y = angle;
            inner.eulerAngles = newRotation;

            Vector3 distance = inner.position - harmonySphere.position;
            float sqrDistance = distance.sqrMagnitude;
            if (sqrDistance > distanceThreshold) {
                rb.AddForce(forceConstantFar * distance.normalized * sqrDistance, ForceMode.Acceleration);
            }
        }
    }

}
