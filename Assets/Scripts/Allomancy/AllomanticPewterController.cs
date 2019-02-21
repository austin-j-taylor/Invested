using UnityEngine;
using System.Collections;

/*
 * Controls all aspects of Allomantic Pewter, including shielding.
 */

public class AllomanticPewterController : MonoBehaviour {

    public MetalReserve PewterReserve { get; private set; }
    public float mT = 2;
    public float mM = 10;
    private void Awake() {
        PewterReserve = gameObject.AddComponent<MetalReserve>();
    }

    private void Update() {
        if(Keybinds.SelectDown()) {
            Debug.Log(Drain(mM, mT));
        }
        Debug.Log(PewterReserve.Mass);
    }

    public void Clear() {
        StopAllCoroutines();
        PewterReserve.SetMass(20);
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
        while(massDrained + deltaMass < totalMass && t < maxtime) {
            massDrained += deltaMass;
            PewterReserve.Mass -= deltaMass;
            
            yield return new WaitForFixedUpdate();

            t += Time.fixedDeltaTime;
            deltaMass = (b - m * t * t) * Time.fixedDeltaTime;
        }

        // Drain remaining amount of mass
        PewterReserve.Mass -= totalMass - massDrained;
    }
}
