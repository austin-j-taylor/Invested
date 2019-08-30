using UnityEngine;
using System.Collections;

/*
 * A "Lock" that, when destroyed, releases power.
 */
public class WireLock : Destructable {

    [SerializeField]
    Powered[] connected = null;

    private bool isBeingDestroyed = false;

    private Magnetic metal;
    private Renderer mount;
    private Animator anim;

    private void Awake() {
        maxHealth = 120;

        metal = GetComponentInChildren<Magnetic>();
        mount = GetComponentInChildren<Renderer>();
        anim = GetComponent<Animator>();
    }

    protected override void Start() {
        base.Start();
        
        foreach (Powered powered in connected) {
            powered.On = true;
        }
    }

    private void FixedUpdate() {
        if (!destroyed) {
            if (isBeingDestroyed) {
                if (metal.IsBeingPushPulled) {
                    Health--;
                } else {
                    isBeingDestroyed = false;
                    anim.SetBool("isBeingDestroyed", false);
                    Health = maxHealth;
                }
            } else {
                if (metal.IsBeingPushPulled) {
                    isBeingDestroyed = true;
                    anim.SetBool("isBeingDestroyed", true);
                    Health--;
                } else {

                }
            }
        }
    }

    // If player Pulls or Pushes on the lock, it is destroyed.
    protected override void Destroy() {
        base.Destroy();
        anim.SetTrigger("destroyed");
        foreach (Powered powered in connected) {
            powered.On = false;
        }

        metal.enabled = false;
    }
}
