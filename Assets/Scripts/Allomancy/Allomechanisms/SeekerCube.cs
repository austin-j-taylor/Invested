using UnityEngine;
using System.Collections;

/*
 * An object that responds to burning metals near it.
 * 
 */
public class SeekerCube : MonoBehaviour {

    private const float radius = 3;
    private const float duration = 5;
    private const float maxRotationSpeedLow = .3f;
    private const float maxRotationSpeedHigh = 30f;
    private const float highDrag = 50f;
    private const float lowDrag = 0f;
    private readonly Vector3 torque = new Vector3(1000, 1000, 1000);
    private readonly Color glowColor = new Color(.75f, .15f, 0f);

    private Rigidbody rb;
    private Renderer[] bronzes;

    private float counter;
    private bool finished;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        bronzes = transform.Find("Frame/Bronzes").GetComponentsInChildren<Renderer>();
        counter = 0;
        finished = false;
    }

    private void LateUpdate() {

        if (counter > duration) {
            if (!finished) {
                GetComponentInChildren<MeshRenderer>().material = GameManager.Material_Ettmetal_Glowing;
                GetComponentInChildren<Light>().intensity = 1.5f;
                EnableEmissions(4);
                rb.angularVelocity = rb.angularVelocity / 2;
                rb.constraints = RigidbodyConstraints.None;
                rb.AddForce(new Vector3(0, -4, 0), ForceMode.VelocityChange);
                finished = true;
            }
        } else {
            bool sought = false;
            foreach (Allomancer allomancer in GameManager.Allomancers) {
                if ((allomancer.transform.position - transform.position).magnitude < radius && allomancer.IsBurning) {
                    sought = true;
                }
            }
            if (sought) { // a nearby allomancer is burning
                EnableEmissions(3 * Mathf.LinearToGammaSpace(Mathf.Pow(counter / duration, 4)));
                counter += Time.deltaTime;
                rb.angularDrag = Mathf.Lerp(highDrag, lowDrag, counter / duration);

            } else { // No nearby burning

                foreach (Renderer rend in bronzes) {
                    rend.material.DisableKeyword("_EMISSION");
                }

                counter = 0;
                rb.angularDrag = highDrag;
            }
        }

    }

    private void FixedUpdate() {
        if (finished) {
            rb.AddForce(new Vector3(0, 9.81f, 0), ForceMode.Acceleration);
        } else {
            rb.AddTorque(transform.rotation * torque * (1 + 2 * counter / duration)); 
        }
    }

    // Enables the emissions of the material specified by mat.
    private void EnableEmissions(float intensity) {
        foreach (Renderer rend in bronzes) {
            rend.material.SetColor("_EmissionColor", glowColor * intensity);
            rend.material.EnableKeyword("_EMISSION");
        }
    }
}
