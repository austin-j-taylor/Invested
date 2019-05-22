using UnityEngine;

public class Vials : MonoBehaviour {

    [SerializeField]
    private bool persistent = true;
    [SerializeField]
    private bool fillIron = true;
    [SerializeField]
    private bool fillSteel = true;
    [SerializeField]
    private bool fillPewter = true;
    [SerializeField]
    private float volume = 150;

    readonly Color iron = new Color(0, .5f, 1);
    readonly Color steel = new Color(.85f, .06f, .06f);
    readonly Color pewter = new Color(1, .5f, .2f);

    Animator anim;
    Light light;

    bool empty = false;

    // Must update every frame after animator does its hardest to fight me
    private void LateUpdate() {
        if (!empty) {
            if (fillIron) {
                if (!fillSteel && !fillPewter) {
                    // pure iron
                    light.color = iron;
                }
            } else if (fillSteel) {
                if (!fillPewter) {
                    // pure steel
                    light.color = steel;
                }
            } else if (fillPewter) {
                // pure pewter
                light.color = pewter;
            }
        }
    }

    private void Awake() {
        anim = GetComponentInParent<Animator>();
        light = GetComponentInParent<Light>();

        if(fillIron) {
            if(fillSteel) {
                if(fillPewter) {
                    anim.Play("Vial_IronSteelPewter", anim.GetLayerIndex("Light"));
                } else {
                    anim.Play("Vial_IronSteel", anim.GetLayerIndex("Light"));
                }
            } else if(!fillPewter) {
                // pure iron
                light.color = iron;
            }
        } else if(fillSteel) {
            if(!fillPewter) {
                // pure steel
                light.color = steel;
            }
        } else if(fillPewter) {
            // pure pewter
            light.color = pewter;
        }

    }

    private void OnCollisionEnter(Collision collision) {
        if (!empty) {
            Collider other = collision.collider;
            if (!other.isTrigger && other.CompareTag("Player")) {
                if (fillIron) {
                    Player.PlayerIronSteel.IronReserve.Fill(volume, persistent ? volume : 0);
                    HUD.MetalReserveMeters.AlertIron();
                }
                if (fillSteel) {
                    Player.PlayerIronSteel.SteelReserve.Fill(volume, persistent ? volume : 0);
                    HUD.MetalReserveMeters.AlertSteel();
                }
                if (fillPewter) {
                    Player.PlayerPewter.PewterReserve.Fill(volume, persistent ? volume : 0);
                    HUD.MetalReserveMeters.AlertPewter();
                }

                if (persistent) {
                    anim.SetTrigger("Empty");
                } else {
                    empty = true;
                    Destroy(transform.parent.Find("Fluid").gameObject);
                    anim.enabled = false;
                    light.enabled = false;
                    Collider col = GetComponent<Collider>();
                    col.isTrigger = false;
                    //gameObject.layer = LayerMask.NameToLayer("Ignore Player");
                    Rigidbody rb = gameObject.AddComponent<Rigidbody>();
                    rb.mass = 1;

                }
            }
        }
    }
}
