using UnityEngine;
using System.Collections;

/*
 * An environmental object that can be destroyed.
 */
public class Destructable : MonoBehaviour {

    protected double maxHealth = 100;

    protected bool destroyed = false;

    protected double health;
    public virtual double Health {
        get {
            return health;
        }
        set {
            health = value;
            if (health <= 0) {
                health = 0;
                if(!destroyed)
                    Destroy();
            }
        }
    }

    protected virtual void Start() {
        health = maxHealth;
    }

    protected virtual void Destroy() {
        destroyed = true;

    }
}
