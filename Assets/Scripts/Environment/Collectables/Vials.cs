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
    new Light light;

    // If I didn't have to do this every frame, I would. Unity Animator is extremely annoying.
    private void LateUpdate() {
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

    private void OnTriggerEnter(Collider other) {
        if(!other.isTrigger && other.CompareTag("Player")) {
            if(fillIron) {
                Player.PlayerIronSteel.IronReserve.Fill(volume, persistent ? volume : 0);
                HUD.MetalReserveMeters.AlertIron();
            }
            if(fillSteel) {
                Player.PlayerIronSteel.SteelReserve.Fill(volume, persistent ? volume : 0);
                HUD.MetalReserveMeters.AlertSteel();
            }
            if(fillPewter) {
                Player.PlayerPewter.PewterReserve.Fill(volume, persistent ? volume : 0);
                HUD.MetalReserveMeters.AlertPewter();
            }

            if (!persistent)
                Destroy(gameObject);
        }
    }
}
