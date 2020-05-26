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
    private AudioSource audioDestroying, audioDestroyed, audioRepairing;

    public override double Health {
        set {
            health = value;
            if (health <= 0) {
                health = 0;
                if(repairing) {
                    // Do not destroy this; however, disable the magnetic and begin repairing.
                    DestroyButThenRepair();
                } else if (On)
                    Destroy();
            }
        }
    }


    protected override void Awake() {
        maxHealth = 120;
        base.Awake();

        metal = GetComponentInChildren<Magnetic>();
        mount = GetComponentInChildren<Renderer>();
        anim = GetComponent<Animator>();
        AudioSource[] sources = GetComponents<AudioSource>();
        audioDestroying = sources[0];
        audioDestroyed = sources[1];
        audioRepairing = sources[2];
    }

    private void Start() {
        anim.SetBool("repairing", repairing);

        On = true;
    }

    private void LateUpdate() {
        if (On) {
            if (isBeingDestroyed) {
                if (metal.IsBeingPushPulled) {
                    Health -= Time.deltaTime * 60;
                } else {
                    isBeingDestroyed = false;
                    anim.SetBool("isBeingDestroyed", false);
                    audioDestroying.Stop();
                    Health = maxHealth;
                }
            } else {
                if (metal.IsBeingPushPulled) {
                    isBeingDestroyed = true;
                    anim.SetBool("isBeingDestroyed", true);
                    audioDestroying.Play();
                    Health -= Time.deltaTime * 60;
                }
            }
        }
    }

    // If player Pulls or Pushes On the lock, it is destroyed.
    protected override void Destroy() {
        base.Destroy();
        anim.SetTrigger("destroyed");
        audioDestroyed.Play();
        On = false;

        metal.enabled = false;
    }

    // If player Pulls or Pushes On the lock, it is destroyed - until it repairs itself
    private void DestroyButThenRepair() {
        anim.SetTrigger("destroyed");
        audioDestroyed.Play();
        audioRepairing.Play();

        On = false;
        metal.enabled = false;
        anim.speed = 1f / timeToRepair;
    }

    private void Repair() {
        metal.enabled = true;
        On = true;
        health = maxHealth;

        anim.speed = 1;
    }
}
