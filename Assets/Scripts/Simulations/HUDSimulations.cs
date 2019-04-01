using UnityEngine;
using UnityEngine.SceneManagement;

public class HUDSimulations : MonoBehaviour {

    public static Transform Duel, CoinWall, CoinGround;

    private Transform simulations;

    private void Awake() {
        simulations = transform.GetChild(0);
        Duel = simulations.GetChild(0);
        CoinWall = simulations.GetChild(1);
        CoinGround = simulations.GetChild(2);

        SceneManager.sceneLoaded += RefreshHUD;
        RefreshHUD(SceneManager.GetActiveScene());
    }

    private void RefreshHUD(Scene scene, LoadSceneMode mode = 0) {
        Debug.Log(mode + " " + scene.buildIndex);
        if (mode == LoadSceneMode.Single) { // Not loading all of the scenes, as it does at startup
            simulations.gameObject.SetActive(false);

            switch (scene.buildIndex) {
                case SceneSelectMenu.sceneSimulationDuel:
                    simulations.gameObject.SetActive(true);
                    Duel.gameObject.SetActive(true);
                    CoinWall.gameObject.SetActive(false);
                    CoinGround.gameObject.SetActive(false);
                    FindObjectOfType<Simulation>().GetComponent<Simulation>().StartSimulation();
                    break;
                case SceneSelectMenu.sceneSimulationWall:
                    simulations.gameObject.SetActive(true);
                    Duel.gameObject.SetActive(false);
                    CoinWall.gameObject.SetActive(true);
                    CoinGround.gameObject.SetActive(false);
                    FindObjectOfType<Simulation>().StartSimulation();
                    break;
                case SceneSelectMenu.sceneSimulationGround:
                    simulations.gameObject.SetActive(true);
                    Duel.gameObject.SetActive(false);
                    CoinWall.gameObject.SetActive(false);
                    CoinGround.gameObject.SetActive(true);
                    FindObjectOfType<Simulation>().StartSimulation();
                    break;
                default: // A normal, non-simulation scene was opened
                    simulations.gameObject.SetActive(false);
                    break;
            }
        }
    }
}
