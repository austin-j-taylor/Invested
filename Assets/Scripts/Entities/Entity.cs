﻿using UnityEngine;
using System.Collections;

/*
* Controls the health of an entity. Makes the entity take damage whenever it collides with another object.
*/
public class Entity : MonoBehaviour {

    protected const float fallDamageSquareSpeedThreshold = 150; // any fall speed above this -> painful

    public string EntityName { get; protected set; } = "";
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
    public bool Hostile { get; protected set; } = false;

    //protected float hitstun;
    //private float lastHitTime;
    protected bool isDead;
    protected BoxCollider[] hitboxes;
    public Rigidbody Rb { get; private set; }
    // Does not account for the rotation of the object.
    public Vector3 FuzzyGlobalCenterOfMass => Rb.transform.position + Rb.centerOfMass;
    // A trigger that encompasses this object. Its bounds define where the reticles go when targeting this.
    public Collider BoundingBox { get; protected set; } = null;

    private bool hitThisFrame;

    protected virtual void Start() {
        hitboxes = GetComponentsInChildren<BoxCollider>();
        Rb = GetComponentInChildren<Rigidbody>();
        Transform boundingBox = transform.Find("BoundingBox");
        if(boundingBox != null)
            BoundingBox = boundingBox.GetComponent<Collider>();
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

    private void OnDestroy() {
        GameManager.EntitiesInScene.Remove(this);
    }
    private void OnDisable() {
        GameManager.EntitiesInScene.Remove(this);
    }
    private void OnEnable() {
        GameManager.EntitiesInScene.Add(this);
    }
}
