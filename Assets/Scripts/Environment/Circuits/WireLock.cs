using UnityEngine;
using System.Collections;

/*
 * A "Lock" that, when destroyed, releases power.
 */
public class WireLock : Source {

    [SerializeField]
    private bool repairing = false;
    [SerializeField]
    private float timeToRepair = 1;


    private bool isBeingDestroyed = false;

    private Magnetic metal;
    private Renderer mount;
    private Animator anim;

    public override double Health {
        set {
            health = value;
            if (health <= 0) {
                health = 0;
                if(repairing) {
                    // Do not destroy this; however, disable the magnetic and begin repairing.
                    DestroyButThenRepair();
                } else if (on)
                    Destroy();
            }
        }
    }


    private void Awake() {
        maxHealth = 120;

        metal = GetComponentInChildren<Magnetic>();
        mount = GetComponentInChildren<Renderer>();
        anim = GetComponent<Animator>();
    }

    protected override void Start() {
        base.Start();
        anim.SetBool("repairing", repairing);

        PowerConnected(true);
    }

    private void LateUpdate() {
        if (on) {
            if (isBeingDestroyed) {
                if (metal.IsBeingPushPulled) {
                    Health -= Time.deltaTime * 60;
                } else {
                    isBeingDestroyed = false;
                    anim.SetBool("isBeingDestroyed", false);
                    Health = maxHealth;
                }
            } else {
                if (metal.IsBeingPushPulled) {
                    isBeingDestroyed = true;
                    anim.SetBool("isBeingDestroyed", true);
                    Health -= Time.deltaTime * 60;
                }
            }
        }
    }

    // If player Pulls or Pushes on the lock, it is destroyed.
    protected override void Destroy() {
        base.Destroy();
        anim.SetTrigger("destroyed");
        PowerConnected(false);

        metal.enabled = false;
    }

    // If player Pulls or Pushes on the lock, it is destroyed - until it repairs itself
    private void DestroyButThenRepair() {
        anim.SetTrigger("destroyed");
        PowerConnected(false);

        on = false;
        metal.enabled = false;
        anim.speed = 1f / timeToRepair;
    }

    private void Repair() {
        metal.enabled = true;
        on = true;
        health = maxHealth;
        PowerConnected(true);

        anim.speed = 1;
    }
}
