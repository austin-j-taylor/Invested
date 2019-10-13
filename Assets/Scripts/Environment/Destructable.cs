﻿using UnityEngine;
using System.Collections;

/*
 * An Investiture circuit element that can be destroyed.
 */
public class Destructable : Powered {

    protected double maxHealth = 100;

    protected double health;
    public virtual double Health {
        get {
            return health;
        }
        set {
            health = value;
            if (health <= 0) {
                health = 0;
                if(On)
                    Destroy();
            }
        }
    }

    protected virtual void Start() {
        health = maxHealth;
        On = true;
    }

    protected virtual void Destroy() {
        On = false;
    }
}
