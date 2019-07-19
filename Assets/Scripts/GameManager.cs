using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using VolumetricLines;
/*
 * At startup, loads the Title Screen scene
 * Stores all Resources.
 * Stores string fields used by TriggerBeadPopups.
 */
public class GameManager : MonoBehaviour {

    // Sub-controllers
    public static GraphicsController GraphicsController { get; private set; }

    // Resurces
    public static Material Material_Ettmetal_Glowing { get; private set; }
    public static Font Font_Heebo { get; private set; }
    public static VolumetricLineBehavior MetalLineTemplate { get; private set; }
    public static VolumetricLineStripBehavior MetalLineStripTemplate { get; private set; }

    // Objects
    public static Coin Prefab_Coin;

    // Holds all Magnetics and Allomancers in scene
    public static List<Allomancer> Allomancers { get; private set; }
    public static List<Magnetic> MagneticsInScene { get; private set; }

    // Masks that represent certain layers to ignore for Raycasting
    public static int Layer_IgnorePlayer { get; private set; }
    public static int Layer_IgnoreCamera { get; private set; }
    public static int Layer_BlueLines { get; private set; }
    public static int Layer_BlueLinesVisible { get; private set; }

    void Awake() {
        GraphicsController = GetComponent<GraphicsController>();

        Material_Ettmetal_Glowing = Resources.Load<Material>("Materials/CompilationShaders/Ettmetal_glowing");
        Font_Heebo = Resources.Load<Font>("Fonts/Heebo-Medium");
        MetalLineTemplate = Resources.Load<VolumetricLineBehavior>("MetalLineTemplate");
        MetalLineStripTemplate = Resources.Load<VolumetricLineStripBehavior>("MetalLineStripTemplate");

        Prefab_Coin = Resources.Load<Coin>("Objects/Imperial1-Boxing");

        Allomancers = new List<Allomancer>();
        MagneticsInScene = new List<Magnetic>();
        Layer_IgnorePlayer = ~((1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Ignore Player")));
        Layer_IgnoreCamera = ~((1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Ignore Camera")) | (1 << LayerMask.NameToLayer("Ignore Player")) | (1 << LayerMask.NameToLayer("Coin")));
        Layer_BlueLines = LayerMask.NameToLayer("Blue Lines");
        Layer_BlueLinesVisible = LayerMask.NameToLayer("Blue Lines Visible");
        SceneManager.sceneUnloaded += Clear;
    }

    private void Start() {
        SceneManager.LoadScene(SceneSelectMenu.sceneTitleScreen);
    }

    private void Clear(Scene scene) {
        MagneticsInScene = new List<Magnetic>();
    }

    public static void AddAllomancer(Allomancer allomancer) {
        Allomancers.Add(allomancer);
    }

    public static void RemoveAllomancer(Allomancer allomancer) {
        Allomancers.Remove(allomancer);
    }

    public static void AddMagnetic(Magnetic magnetic) {
        MagneticsInScene.Add(magnetic);
    }

    public static void RemoveMagnetic(Magnetic magnetic) {
        // Remove from all allomancers
        foreach (Allomancer allomancer in Allomancers) {
            AllomanticIronSteel ironSteel = allomancer.GetComponent<AllomanticIronSteel>();
            if (ironSteel) {
                ironSteel.RemoveTarget(magnetic);
            }
        }
        MagneticsInScene.Remove(magnetic);
    }
}
