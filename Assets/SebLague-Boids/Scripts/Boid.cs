using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour {

    BoidSettings settings;

    // To update:
    Vector3 acceleration;

    // Cached
    Material material;
    Transform target;
    Rigidbody rb;

    void Awake() {
        material = transform.GetComponentInChildren<MeshRenderer>().material;
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(BoidSettings settings, Transform target) {
        this.target = target;
        this.settings = settings;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        rb.velocity = transform.forward * startSpeed;
    }

    public void SetColour(Color col) {
        if (material != null) {
            material.color = col;
        }
    }

    public void FixedUpdateBoid(Vector3 avgFlockHeading, Vector3 avgAvoidanceHeading, Vector3 centreOfFlockmates, int numPerceivedFlockmates) {
        Vector3 acceleration = Vector3.zero;

        if (target != null) {
            Vector3 offsetToTarget = (target.position - transform.position);
            acceleration = SteerTowards(offsetToTarget) * settings.targetWeight;
        }

        if (numPerceivedFlockmates != 0) {
            centreOfFlockmates /= numPerceivedFlockmates;

            Vector3 offsetToFlockmatesCentre = (centreOfFlockmates - transform.position);

            Vector3 alignmentForce = SteerTowards(avgFlockHeading) * settings.alignWeight;
            Vector3 cohesionForce = SteerTowards(offsetToFlockmatesCentre) * settings.cohesionWeight;
            Vector3 seperationForce = SteerTowards(avgAvoidanceHeading) * settings.seperateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += seperationForce;
        }

        if (IsHeadingForCollision()) {
            Vector3 collisionAvoidDir = ObstacleRays();
            Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        rb.AddForce(acceleration, ForceMode.Acceleration);
        //float sqrSpeed = rb.velocity.sqrMagnitude;
        //if (sqrSpeed < settings.minSpeed * settings.minSpeed) {
        //    rb.velocity = rb.velocity.normalized * settings.minSpeed;
        //} else if (sqrSpeed > settings.maxSpeed * settings.maxSpeed) {
        //    rb.velocity = rb.velocity.normalized * settings.maxSpeed;
        //}
        transform.forward = rb.velocity;
    }

    bool IsHeadingForCollision() {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, settings.boundsRadius, transform.forward, out hit, settings.collisionAvoidDst, settings.obstacleMask)) {
            return true;
        } else { }
        return false;
    }

    Vector3 ObstacleRays() {
        Vector3[] rayDirections = BoidHelper.directions;

        for (int i = 0; i < rayDirections.Length; i++) {
            Vector3 dir = transform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(transform.position, dir);
            if (!Physics.SphereCast(ray, settings.boundsRadius, settings.collisionAvoidDst /*, settings.obstacleMask*/)) {
                return dir;
            }
        }

        return transform.forward;
    }

    Vector3 SteerTowards(Vector3 vector) {
        Vector3 v = vector.normalized * settings.maxSpeed - rb.velocity;
        return Vector3.ClampMagnitude(v, settings.maxSteerForce);
    }

}