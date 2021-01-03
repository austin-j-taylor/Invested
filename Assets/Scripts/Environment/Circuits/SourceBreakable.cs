using UnityEngine;
using System.Collections;

/*
 * A Source that can be destroyed to turn on/off, and possibly repair itself over time.
 */
public class SourceBreakable : Source {

    [SerializeField]
    protected bool repairWhenDestroyed = false;
    [SerializeField]
    protected float timeToDestroy = 1; // seconds
    [SerializeField]
    protected float timeToRepair = 1; // seconds

    protected bool isBeingDestroyed = false;

    private Animator anim;
    private AudioSource audioDestroying, audioDestroyed, audioRepairing;

    public override bool On {
        set {
            if (value == OnByDefault && On != OnByDefault) {
                base.On = value;
                Repair();
            }  else
                base.On = value;
        }
    }


    protected override void Awake() {
        base.Awake();

        anim = GetComponent<Animator>();
        AudioSource[] sources = GetComponents<AudioSource>();
        audioDestroying = sources[0];
        audioDestroyed = sources[1];
        audioRepairing = sources[2];
    }

    private void Start() {
        anim.SetBool("repairing", repairWhenDestroyed);
    }

    private IEnumerator StartDestroyingRoutine() {
        if (timeToDestroy == 0) {
            Break();
        } else {
            isBeingDestroyed = true;
            anim.SetBool("isBeingDestroyed", true);
            audioDestroying.Play();
            Health -= Time.deltaTime;
            while (health > 0) {
                ContinueDestroying();
                yield return null;
            }
        }

        if(repairWhenDestroyed) {
            // Repairing
            audioRepairing.Play();
            anim.speed = 1 / timeToRepair;

            while (health < MaxHealth) {
                ContinueRepairing();
                yield return null;
            }
            Repair();
        }
    }

    protected void StartDestroying() {
        if(On == OnByDefault)
            StartCoroutine(StartDestroyingRoutine());
    }
    protected virtual void ContinueDestroying() {
        Health -= Time.deltaTime / timeToDestroy;
    }
    protected virtual void ContinueRepairing() {
        Health += Time.deltaTime / timeToRepair;
    }
    protected void CeaseDestroying() {
        StopAllCoroutines();
        isBeingDestroyed = false;
        anim.SetBool("isBeingDestroyed", false);
        audioDestroying.Stop();
        Health = timeToRepair;
    }

    // If player Pulls or Pushes On the lock, it is destroyed.
    protected override void Break() {
        base.Break();
        audioDestroyed.Play();
        anim.SetTrigger("destroyed");
    }

    protected virtual void Repair() {
        audioRepairing.Stop();
        On = OnByDefault;
        health = timeToRepair;

        anim.speed = 1;
    }
}
