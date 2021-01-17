using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the HUD element showing the number of coins the player has.
/// </summary>
public class ThrowingAmmoMeter : MonoBehaviour {

    private const float timeToFade = 3;

    private Animator anim;
    private Text coinCountText;
    private float timeLastChanged;
    private int lastCount;

    private void Awake() {
        anim = GetComponent<Animator>();
        coinCountText = GetComponentInChildren<Text>();
    }

    private void LateUpdate() {

        if (lastCount != Player.PlayerInstance.CoinHand.Pouch.Count) {
            timeLastChanged = Time.time;
        }

        if (Player.PlayerIronSteel.Mode == PrimaPullPushController.ControlMode.Coinshot) {
            anim.SetBool("IsVisible", true);
        } else {
            anim.SetBool("IsVisible", Time.time - timeLastChanged < timeToFade);
        }

        coinCountText.text = Player.PlayerInstance.CoinHand.Pouch.Count.ToString();

        lastCount = Player.PlayerInstance.CoinHand.Pouch.Count;
    }

    public void Clear() {
        timeLastChanged = -100;
        lastCount = 0;
        anim.SetBool("IsVisible", false);
        anim.Play("MetalReserve_Invisible", anim.GetLayerIndex("Visibility"));
    }

    public void Alert(Player.CoinMode mode) {
        timeLastChanged = Time.time;
        switch (mode) {
            case Player.CoinMode.Semi:
                anim.Play("ThrowableAmmo_Semi", anim.GetLayerIndex("Image"));
                break;
            case Player.CoinMode.Full:
                anim.Play("ThrowableAmmo_Full", anim.GetLayerIndex("Image"));
                break;
            case Player.CoinMode.Spray:
                anim.Play("ThrowableAmmo_Spray", anim.GetLayerIndex("Image"));
                break;
        }
    }
}
