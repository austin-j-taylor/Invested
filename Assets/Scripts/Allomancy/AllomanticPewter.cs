using UnityEngine;
using System.Collections;

/*
 * Controls all aspects of Allomantic Pewter.
 * Controls the particle system associated with using pewter.
 */

[RequireComponent(typeof(Rigidbody))]
public class AllomanticPewter : Allomancer {

    protected const float gramsPewterPerSecondSprint = .5f;
    protected const float gramsPewterPerFall = 2f;
    protected const float timePewterPerFall = 1f;

    public MetalReserve PewterReserve { get; private set; }
    public bool IsSprinting { get; protected set; } = false;
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
    protected ParticleSystem particleSystem;
    protected Quaternion particleDirection;

    protected virtual void Awake() {
        PewterReserve = gameObject.AddComponent<MetalReserve>();
        rb = GetComponent<Rigidbody>();
        particleSystem = transform.parent.GetComponentInChildren<ParticleSystem>();

        GameManager.AddAllomancer(this);
    }

    public override void Clear() {
        IsSprinting = false;
        StopAllCoroutines();
        base.Clear();
    }

    protected virtual void FixedUpdate() {
        if (IsSprinting) {
            PewterReserve.Mass -= gramsPewterPerSecondSprint * Time.fixedDeltaTime;
        }
        // IsBurning mass drain is done through Drain()
    }

    /*
     * Drains totalMass grams of pewter over maxtime seconds.
     * This essentially finds a curve such that:
     *  - totalMass will be drained from the reserve by the time that:
     *  - maxtime seconds have passed since this method was called
     *  
     *  If the Allomancer does not have enough pewter, this returns false.
     */
    public bool Drain(float totalMass, float maxtime) {
        if (PewterReserve.Mass < totalMass)
            return false;
        StartCoroutine(Burst(totalMass, maxtime));
        return true;
    }

    private IEnumerator Burst(float totalMass, float maxtime) {
        float massDrained = 0;
        float t = 0;
        float b = totalMass / maxtime * 1.5f;
        float m = b / (maxtime * maxtime);
        float deltaMass = b * Time.fixedDeltaTime;
        // Because this is not actually a continuous function, checking t < maxTime will
        // not guarantee that the right amount of mass is consumed.
        // Thus:
        while (massDrained + deltaMass < totalMass && t < maxtime) {
            IsDraining = true; // Repeatedly assigned in case of multiple coroutines are running at once and one finishes, setting IsDraining to false
            massDrained += deltaMass;
            PewterReserve.Mass -= deltaMass;

            yield return new WaitForFixedUpdate();

            t += Time.fixedDeltaTime;
            deltaMass = (b - m * t * t) * Time.fixedDeltaTime;
        }

        // Drain remaining amount of mass
        PewterReserve.Mass -= totalMass - massDrained;
        IsDraining = false;
    }

    // When taking damage, attempt to shield it
    public float OnHit(float damage, bool automaticallyShield = false) {
        if (automaticallyShield || PewterReserve.IsDraining) {
            Drain(gramsPewterPerFall, timePewterPerFall);
            return 0;
        } else {
            return damage;
        }
    }

    /*
     * Show particle effects of hitting a surface.
     */
    public void HitSurface(Vector3 normal) {
        particleDirection = Quaternion.LookRotation(normal);
        particleSystem.transform.rotation = particleDirection;
        particleSystem.Play();
    }
}
