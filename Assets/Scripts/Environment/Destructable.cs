using UnityEngine;
using System.Collections;

/*
 * An Investiture circuit element that can be destroyed.
 */
public class Destructable : Powered {

    [SerializeField]
    protected bool OnByDefault = false;

    protected double MaxHealth { get; private set; } = 1;

    protected double health;
    public virtual double Health {
        get {
            return health;
        }
        set {
            health = value;
            if (health <= 0) {
                health = 0;
                if(On == OnByDefault)
                    Break();
            }
        }
    }

    protected virtual void Awake() {
        health = MaxHealth;
        On = OnByDefault;
    }

    protected virtual void Break() {
        health = 0;
        On = !OnByDefault;
    }
}
