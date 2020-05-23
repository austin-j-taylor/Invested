using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAnchored : NonPlayerPushPullController {

    [SerializeField]
    private Magnetic projectile = null;
    [SerializeField]
    private Transform projectileSpawnLocation = null;
    [SerializeField]
    private float refillSpeed = .125f;
    [SerializeField]
    private float radiusOfDetection = 20;
    [SerializeField]
    private float radiusOfAwake = 40;
    [SerializeField]
    private double steelReserveCapacity = .25;
    [SerializeField]
    private int maxPushRange = 5;
    [SerializeField]
    private float strength = 2;

    private Transform swivel, neck, neckEnd;
    private Renderer tubesRenderer;
    private MaterialPropertyBlock fillBlock;

    private Transform target;

    protected override void Awake() {
        base.Awake();

        swivel = transform.Find("Armature/Body/Swivel");
        neck = swivel.Find("Neck 1");
        neckEnd = neck.Find("Neck_end");
        tubesRenderer = transform.Find("Rifle/Tubes/Tube").GetComponent<Renderer>();
        fillBlock = new MaterialPropertyBlock();
    }

    private void Start() {
        rb.centerOfMass = transform.InverseTransformPoint(neckEnd.position);
        SteelReserve.IsEndless = false;

        target = Player.PlayerInstance.transform;
    }

    protected override void FixedUpdate() {
        SteelReserve.Capacity = steelReserveCapacity;
        PushTargets.MaxRange = maxPushRange;
        Strength = strength;

        if (IsActive()) {
            SteelReserve.Mass += Time.deltaTime * refillSpeed;
            fillBlock.SetFloat("_Fill", (float)(SteelReserve.Mass / steelReserveCapacity));
            tubesRenderer.SetPropertyBlock(fillBlock);

            if (SearchForTarget()) {
                TrackTarget();
                if (SteelReserve.Mass >= steelReserveCapacity) {
                    ShootTarget();
                }
            }
        }

        base.FixedUpdate();
    }

    // If the turret is active
    private bool IsActive() {
        return ((target.transform.position - transform.position).magnitude < radiusOfAwake);
    }
    // Returns true if target is found
    private bool SearchForTarget() {
        // Check for LOS
        Vector3 direction = target.transform.position - neck.position;
        Debug.DrawRay(neck.position, direction);
        if (Physics.Raycast(neck.position, direction, out RaycastHit hit)) {
            if(hit.transform == target) {
                return ((direction).magnitude < radiusOfDetection);
            }
        }
        return false;
    }
    private void TrackTarget() {
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

    private void ShootTarget() {
        PushTargets.Clear();
        PushTargets.AddTarget(Instantiate(projectile, projectileSpawnLocation.position, projectileSpawnLocation.rotation), false);
        IsBurning = true;
        SteelPushing = true;
        SteelBurnPercentageTarget = 1;
    }
}
