using UnityEngine;

public class Vials : MonoBehaviour {

    [SerializeField]
    private bool persistent;
    [SerializeField]
    private float volume;
    
    private void OnTriggerEnter(Collider other) {
        if(!other.isTrigger && other.CompareTag("Player")) {
            Player.PlayerIronSteel.IronReserve.Fill(volume);
            Player.PlayerIronSteel.SteelReserve.Fill(volume);
            Player.PlayerPewter.PewterReserve.Fill(volume);
            HUD.MetalReserveMeters.AlertIron();
            HUD.MetalReserveMeters.AlertSteel();
            HUD.MetalReserveMeters.AlertPewter();

            if (!persistent)
                Destroy(gameObject);
        }
    }
}
