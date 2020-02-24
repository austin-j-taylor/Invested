using UnityEngine;
using System.Collections;

/*
 * Controls all aspects of Allomantic Pewter.
 * Controls the particle system associated with using pewter.
 * 
 * Player-specific pewter management is in PlayerMovementController.
 */

[RequireComponent(typeof(Rigidbody))]
public class AllomanticPewter : Allomancer {

    protected const double gramsPewterPerSecondSprint = .500f;
    protected const double gramsPewterPerSecondRefill = .250f;
    protected const double gramsPewterPerFall = .25f;
    protected const float timePewterPerFall = .75f;

    public MetalReserve PewterReserve { get; private set; }
    private bool isSprinting = false;
    public bool IsSprinting {
        get {
            return isSprinting;
        }
        protected set {
            if (isSprinting && !value) {
                GameManager.AudioManager.Stop_pewter();
            } else if (value) {
                GameManager.AudioManager.Play_pewter();
            }
            isSprinting = value;
        }
    }
    public bool IsDraining { get; protected set; } = false;
    public override bool IsBurning {
        get {
            return IsSprinting || IsDraining;
        }
        protected set {
            IsSprinting = false;
            IsDraining = false;
        }
    }

    protected Rigidbody rb;
    private new ParticleSystem particleSystem;
    protected Quaternion particleDirection;

    // The mesh that has a "shielding" effect that flashes when taking damage
    [SerializeField]
    private MeshRenderer shieldRenderer = null;
    private Material shieldMaterial = null;
    private Quaternion shieldRotation;

    protected virtual void Awake() {
        PewterReserve = gameObject.AddComponent<MetalReserve>();
        PewterReserve.Capacity = 1; // 1 gram of pewter investiture that regenerates
        rb = GetComponent<Rigidbody>();
        particleSystem = transform.parent.GetComponentInChildren<ParticleSystem>();
        GameManager.AddAllomancer(this);
        // find the shields' glowy material
        for (int i = 0; i < shieldRenderer.materials.Length; i++) {
            if (shieldRenderer.materials[i].name.Equals("PewterShock (Instance)")) {
                shieldMaterial = shieldRenderer.materials[i];
            }
        }
    }

    public override void Clear() {
        IsSprinting = false;
        PewterReserve.IsBurnedOut = false;
        shieldMaterial.SetFloat("_HitTime", -1); // off
        StopAllCoroutines();
        PewterReserve.Refill();
        base.Clear();
    }

    protected virtual void FixedUpdate() {
        if (PewterReserve.IsBurnedOut) {
            PewterReserve.Mass += gramsPewterPerSecondRefill * Time.fixedDeltaTime;
            if (PewterReserve.Mass >= PewterReserve.Capacity) {
                PewterReserve.IsBurnedOut = false;
            }
        } else {
            if (IsSprinting) {
                PewterReserve.Mass -= gramsPewterPerSecondSprint * Time.fixedDeltaTime;
            }
            // Regenerate pewter conditionally
            if (!PewterReserve.IsFull) {
                double delta = PewterReserve.Capacity / 2 * Time.fixedDeltaTime;

                if (PewterReserve.Mass < PewterReserve.Capacity / 2 - delta) {
                    PewterReserve.Mass += delta;
                } else {
                    delta /= 2;
                    if (PewterReserve.Mass > PewterReserve.Capacity / 2 - delta) {
                        PewterReserve.Mass += delta;
                    } else {
                        PewterReserve.Mass = PewterReserve.Capacity / 2;
                    }
                }
            }

        }
        // IsBurning mass drain is done through Drain()
    }

    /*
     * Drains totalMass grams of pewter over maxTime seconds.
     * This essentially finds a curve such that:
     *  - totalMass will be drained from the reserve by the time that:
     *  - maxTime seconds have passed since this method was called
     *  
     *  If the Allomancer does not have enough pewter, this returns false.
     */
    public bool Drain(Vector3 sourceLocationLocal, double totalMass, float maxTime) {
        //if (PewterReserve.Mass < totalMass)
        //    return false; // if it can't drain that much, fail
        if (PewterReserve.Mass < totalMass) {
            PewterReserve.IsBurnedOut = true;
        }
        StartCoroutine(Burst(sourceLocationLocal, totalMass, maxTime));
        return true;
    }

    /*
     * Release a burst of pewter.
     * 
     * Causes the shield to light up.
     */
    private IEnumerator Burst(Vector3 sourceLocationLocal, double totalMass, float maxTime) {
        GameManager.AudioManager.Play_pewter_burst();

        // Light up the shield:
        // get closest point on mesh where that happens (for now, assume it's a sphere w/ radius .5)
        sourceLocationLocal = sourceLocationLocal / sourceLocationLocal.magnitude * .5f;
        // Flash the shield
        shieldMaterial.SetVector("_SourcePosition", sourceLocationLocal);
        shieldRotation = Quaternion.identity;

        double massDrained = 0;
        float t = 0;
        double b = totalMass / maxTime * 1.5f;
        double m = b / (maxTime * maxTime);
        double deltaMass = b * Time.fixedDeltaTime;
        // Because this is not actually a continuous function, checking t < maxTime will
        // not guarantee that the right amount of mass is consumed.
        // Thus:
        while (massDrained + deltaMass < totalMass && t + Time.fixedDeltaTime < maxTime) {
            IsDraining = true; // Repeatedly assigned in case of multiple coroutines are running at once and one finishes, setting IsDraining to false

            massDrained += deltaMass;
            PewterReserve.Mass -= deltaMass;

            // Set shield properties
            shieldMaterial.SetFloat("_HitTime", t / maxTime);
            shieldMaterial.SetFloat("_Intensity", (float)((totalMass - massDrained) / totalMass));
            shieldRenderer.transform.rotation = shieldRotation;

            yield return new WaitForFixedUpdate();
            // Evaluate the cumulative function at this time
            // and "set" the reserve to where it should be
            deltaMass = (b * t - m * t * t * t / 3) - massDrained;
            t += Time.fixedDeltaTime;
        }

        // Drain remaining amount of mass
        PewterReserve.Mass -= totalMass - massDrained;
        IsDraining = false;
        shieldMaterial.SetFloat("_HitTime", -1); // off
    }

    // When taking damage, attempt to shield it
    public virtual float OnHit(Vector3 sourceLocationLocal, float damage, bool automaticallyShield = false) {
        if (automaticallyShield || IsBurning) {
            Drain(sourceLocationLocal, gramsPewterPerFall, timePewterPerFall);
            return 0;
        } else {
            return damage;
        }
    }

    /*
     * Show particle effects of hitting a surface.
     */
    public void HitSurface(Vector3 normal) {
        normal.Normalize();
        particleDirection = Quaternion.LookRotation(normal);
        particleSystem.transform.rotation = particleDirection;
        particleSystem.transform.position = transform.position + normal * PlayerMovementController.radius;

        particleSystem.Play();
    }
}
