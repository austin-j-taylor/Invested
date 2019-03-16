using UnityEngine;

public class Vials : MonoBehaviour {

    [SerializeField]
    private bool persistent = true;
    [SerializeField]
    private float volume = 150;
    
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
