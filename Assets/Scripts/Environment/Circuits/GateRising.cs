using UnityEngine;
using System.Collections;

public class GateRising : Powered {

    [SerializeField]
    private float height = 5;

    public override bool On {
        set {
            if (!On && value)
                StartCoroutine(Unlock());
            base.On = value;
        }
    }
    public bool Unlocked = false;
    private IEnumerator Unlock() {
        Unlocked = true;
        GetComponent<AudioSource>().Play();
        Rigidbody rb = GetComponentInChildren<Rigidbody>();
        rb.isKinematic = false;
        NonPlayerPushPullController allomancer = GetComponentInChildren<NonPlayerPushPullController>();
        Magnetic target = GetComponentInChildren<Magnetic>();
        allomancer.AddPushTarget(target);
        allomancer.SteelPushing = true;
        allomancer.SteelBurnPercentageTarget = 1;
        allomancer.BaseStrength = 50;
        allomancer.gameObject.GetComponent<Renderer>().materials[1].CopyPropertiesFromMaterial(GameManager.Material_Steel_lit);

        float distance = (allomancer.transform.position - target.transform.position).sqrMagnitude;
        while (rb.velocity.sqrMagnitude > .01 || distance < height / 2) {
            allomancer.SteelPushing = distance < height * height;
            distance = (allomancer.transform.position - target.transform.position).sqrMagnitude;
            yield return null;
        }
        rb.isKinematic = true;
        allomancer.enabled = false;
    }
}
