using UnityEngine;
using System.Collections;

/*
* Controls the health of an entity. Makes the entity take damage whenever it collides with another object.
*/
public class Entity : MonoBehaviour {

    private double health;
    private double maxHealth;

    public double Health {
        get {
            return health;
        }
        set {
            health = value;
            if (health < 0)
                health = 0;
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
    protected Animator anim;
    protected Rigidbody rb;

    private bool hitThisFrame;

    virtual protected void Start() {
        hitboxes = GetComponentsInChildren<BoxCollider>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponentInChildren<Rigidbody>();
        isDead = false;
        hitThisFrame = false;
        //hitstun = 0f;
        //lastHitTime = 0;
    }

    virtual protected void LateUpdate() {
        hitThisFrame = false;
    }

    private void OnCollisionEnter(Collision collision) {
        if(collision.rigidbody) {
            Vector3 collisionVelocity = collision.rigidbody.velocity - rb.velocity;
            float kineticEnergy = .5f * collision.rigidbody.mass * collisionVelocity.sqrMagnitude;
            OnHit(kineticEnergy / 10);
        } else {

        }
    }

    virtual public void OnHit(float damage) {
        //if (lastHitTime + hitstun < Time.time) {
        if (!hitThisFrame) {
            Health -= damage;
            hitThisFrame = true;
        }
        //    lastHitTime = Time.time;
        //}
    }
}
