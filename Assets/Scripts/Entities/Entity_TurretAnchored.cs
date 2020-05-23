using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity_TurretAnchored : NonPlayerPushPullController {

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
    private Rigidbody targetRb;

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
        targetRb = Player.PlayerIronSteel.rb;
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
    private float lastTimeToReachTarget = 0;
    private IEnumerator CountTimeToReachTarget(float distance, Transform projectile) {
        while((projectile.position - projectileSpawnLocation.position).magnitude < distance && lastTimeToReachTarget < 1) {
            lastTimeToReachTarget += Time.deltaTime;
            yield return null;
        }
        if (lastTimeToReachTarget >= 1)
            lastTimeToReachTarget = 0;
    }
    private void TrackTarget() {
        // Swivel rotation (horizontal)
        // Neck rotation (vertical)
        Vector3 relativeTargetPosition = new Vector3(target.position.x - swivel.position.x, target.position.y - neckEnd.position.y, target.position.z - swivel.position.z);
        relativeTargetPosition += targetRb.velocity * lastTimeToReachTarget;
        float angleDesiredX = Mathf.Atan2(relativeTargetPosition.x, relativeTargetPosition.z) * Mathf.Rad2Deg;
        float angleDesiredY = -Mathf.Atan2(relativeTargetPosition.y, Mathf.Sqrt(relativeTargetPosition.x * relativeTargetPosition.x + relativeTargetPosition.z * relativeTargetPosition.z)) * Mathf.Rad2Deg;

        swivel.transform.eulerAngles = new Vector3(0, angleDesiredX, 0);
        neck.transform.localEulerAngles = new Vector3(angleDesiredY, 0, 0);
    }

    private void ShootTarget() {
        PushTargets.Clear();
        Magnetic newShot = Instantiate(projectile, projectileSpawnLocation.position, projectileSpawnLocation.rotation);
        PushTargets.AddTarget(newShot, false);
        IsBurning = true;
        SteelPushing = true;
        SteelBurnPercentageTarget = 1;
        StopAllCoroutines();
        lastTimeToReachTarget = 0;
        StartCoroutine(CountTimeToReachTarget((target.transform.position - projectileSpawnLocation.position).magnitude, newShot.transform));
    }
}
