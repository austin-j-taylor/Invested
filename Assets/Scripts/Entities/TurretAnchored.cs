using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAnchored : MonoBehaviour
{
    private Transform swivel, neck, neckEnd;
    private Transform target;

    private void Awake() {
        swivel = transform.Find("Armature/Body/Swivel");
        neck = swivel.Find("Neck 1");
        neckEnd = neck.Find("Neck_end");
        target = Player.PlayerInstance.transform;
    }

    private void LateUpdate() {
        // Swivel rotation (horizontal)
        float deltaX = target.position.x - swivel.position.x;
        float deltaZ = target.position.z - swivel.position.z;
        float angle = Mathf.Atan2(deltaX, deltaZ) * Mathf.Rad2Deg;
        swivel.transform.eulerAngles = new Vector3(0, angle, 0);

        // Neck rotation (vertical)
        
        deltaX = target.position.x - neckEnd.position.x;
        deltaZ = target.position.z - neckEnd.position.z;
        angle = Mathf.Atan2(target.position.y - neckEnd.position.y, Mathf.Sqrt(deltaX * deltaX + deltaZ * deltaZ)) * Mathf.Rad2Deg;
        neck.transform.localEulerAngles = new Vector3(-angle, 0, 0);
    }
}
