using UnityEngine;
using UnityEngine.SceneManagement;

public class HUDSimulations : MonoBehaviour {

    private enum SimulationType { Duel, CoinWall, CoinGround }

    public static Transform Duel, CoinWall, CoinGround;

    private Transform simulations;

    private void Start() {
        simulations = transform.GetChild(0);
        Duel = simulations.GetChild(0);
        CoinWall = simulations.GetChild(1);
        CoinGround = simulations.GetChild(2);

        SceneManager.sceneLoaded += RefreshHUD;
        RefreshHUD(SceneManager.GetActiveScene());
    }

    private void RefreshHUD(Scene scene, LoadSceneMode mode = 0) {
        if (mode == LoadSceneMode.Single) { // Not loading all of the scenes, as it does at startup

            switch (scene.buildIndex) {
                case SceneSelectMenu.sceneSimulationDuel:
                    ReadySimulation();
                    Duel.gameObject.SetActive(true);
                    break;
                case SceneSelectMenu.sceneSimulationWall:
                    ReadySimulation();
                    CoinWall.gameObject.SetActive(true);
                    break;
                case SceneSelectMenu.sceneSimulationGround:
                    ReadySimulation();
                    CoinGround.gameObject.SetActive(true);
                    break;
                default: // A normal, non-simulation scene was opened
                    simulations.gameObject.SetActive(false);
                    return;
            }

            HUD.DisableHUD();
            Player.CanControlMovement = false;
            Player.CanControlPushes = false;
            Player.CanControlWheel = false;
            Player.CanControlZinc = true;
            FindObjectOfType<Simulation>().StartSimulation();
        }
    }

    private void ReadySimulation() {
        simulations.gameObject.SetActive(true);
        Duel.gameObject.SetActive(false);
        CoinWall.gameObject.SetActive(false);
        CoinGround.gameObject.SetActive(false);
        Player.CanControlMovement = false;
    }
}
