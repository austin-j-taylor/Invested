using UnityEngine;
using System.Collections;

/*
* Controls the health of an entity. Makes the entity take damage whenever it collides with another object.
*/
public class Entity : MonoBehaviour {

    protected const float fallDamageSquareSpeedThreshold = 150; // any fall speed above this -> painful

    private double health;
    private double maxHealth;

    public double Health {
        get {
            return health;
        }
        set {
            health = value;
            if (health <= 0) {
                health = 0;
                if(!isDead)
                    Die();
            }
        }
    }
    public double MaxHealth {
        get {
            return maxHealth;
        }
        set {
            if (maxHealth == 0) {
                health = value;
            }
            maxHealth = value;
            if (maxHealth < 0)
                maxHealth = 0;
        }
    }

    //protected float hitstun;
    //private float lastHitTime;
    protected bool isDead;
    protected BoxCollider[] hitboxes;
    protected Rigidbody rb;

    private bool hitThisFrame;

    protected virtual void Start() {
        hitboxes = GetComponentsInChildren<BoxCollider>();
        rb = GetComponentInChildren<Rigidbody>();
        isDead = false;
        hitThisFrame = false;
        //hitstun = 0f;
        //lastHitTime = 0;
    }

    protected virtual void LateUpdate() {
        hitThisFrame = false;
    }

    protected virtual void OnCollisionEnter(Collision collision) {
        Vector3 thisNormal = collision.GetContact(0).normal;
        if (Vector3.Project(collision.relativeVelocity, thisNormal).sqrMagnitude > fallDamageSquareSpeedThreshold) {
            OnHit(collision.GetContact(0).point - transform.position, collision.impulse.magnitude);
        }
    }

    public virtual void OnHit(Vector3 sourceLocation, float damage) {
        //if (lastHitTime + hitstun < Time.time) {
        if (!hitThisFrame) {
            Health -= damage;
            hitThisFrame = true;
        }
        //    lastHitTime = Time.time;
        //}
    }

    protected virtual void Die() {
        isDead = true;
    }
}
