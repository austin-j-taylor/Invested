
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using VolumetricLines;
/// <summary>
/// At startup, loads the Title Screen scene.
/// Stores all Resources.
/// </summary>
public class GameManager : MonoBehaviour {

    public enum GameState { Standard, Challenge, Cutscene };

    #region controllers
    // Sub-controllers, transforms
    public static AudioManager AudioManager { get; private set; }
    public static ConversationManager ConversationManager { get; private set; }
    public static GraphicsController GraphicsController { get; private set; }
    public static CloudsManager CloudsManager { get; private set; }
    public static Transform MetalLinesTransform { get; private set; }
    #endregion

    #region resources
    // Resurces
    public static Material Material_MARLmetal_unlit { get; private set; }
    public static Material Material_MARLmetal_lit { get; private set; }
    public static Material Material_Steel_lit { get; private set; }
    public static Font Font_Heebo { get; private set; }
    public static VolumetricLineBehavior MetalLineTemplate { get; private set; }
    public static VolumetricLineStripBehavior MetalLineStripTemplate { get; private set; }

    // Objects
    public static Coin Prefab_Coin;
    #endregion

    // Holds all Magnetics and Allomancers in scene
    public static List<Allomancer> Allomancers { get; private set; } = new List<Allomancer>();
    public static List<Magnetic> MagneticsInScene { get; private set; } = new List<Magnetic>();

    #region layers
    // Masks that represent certain layers to ignore for Raycasting
    public static int Layer_IgnorePlayer { get; private set; }
    public static int Layer_IgnoreCamera { get; private set; }
    public static int Layer_BlueLines { get; private set; }
    public static int Layer_BlueLinesVisible { get; private set; }
    #endregion

    public static GameState State { get; private set; }

    #region clearing
    void Awake() {
        AudioManager = transform.Find("AudioManager").GetComponent<AudioManager>();
        ConversationManager = GetComponent<ConversationManager>();
        GraphicsController = GetComponent<GraphicsController>();
        CloudsManager = GetComponent<CloudsManager>();
        MetalLinesTransform = transform.Find("MetalLines");

        Material_MARLmetal_unlit = Resources.Load<Material>("Materials/MARLmetal_unlit");
        Material_MARLmetal_lit = Resources.Load<Material>("Materials/MARLmetal_lit");
        Material_Steel_lit = Resources.Load<Material>("Materials/Steel_lit");
        Font_Heebo = Resources.Load<Font>("Fonts/Heebo-Medium");
        MetalLineTemplate = Resources.Load<VolumetricLineBehavior>("MetalLineTemplate");
        MetalLineStripTemplate = Resources.Load<VolumetricLineStripBehavior>("MetalLineStripTemplate");

        Prefab_Coin = Resources.Load<Coin>("Objects/Imperial1-Boxing");

        Layer_IgnorePlayer = ~((1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Ignore Player")));
        Layer_IgnoreCamera = ~((1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Ignore Camera")) | (1 << LayerMask.NameToLayer("Ignore Player")) | (1 << LayerMask.NameToLayer("Coin")) | (1 << LayerMask.NameToLayer("Boid")));
        Layer_BlueLines = LayerMask.NameToLayer("Blue Lines");
        Layer_BlueLinesVisible = LayerMask.NameToLayer("Blue Lines Visible");

        State = GameState.Standard;

        SceneManager.sceneUnloaded += ClearBeforeSceneChange;
    }

    private void Start() {
        SceneManager.LoadScene(SceneSelectMenu.sceneTitleScreen);
    }

    /// <summary>
    /// Called when exiting a scene
    /// </summary>
    /// <param name="scene"></param>
    private void ClearBeforeSceneChange(Scene scene) {
        MagneticsInScene = new List<Magnetic>();
        foreach (Transform child in MetalLinesTransform) {
            Destroy(child.gameObject);
        }
        AudioManager.Clear();
    }
    #endregion

    #region allomancy
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
                if (ironSteel.UsingBubble) {
                    ironSteel.RemoveBubbleTarget(magnetic);
                }
            }
        }
        MagneticsInScene.Remove(magnetic);
    }
    #endregion

    // Changes the overall game state
    public static void SetState(GameState newState) {
        switch (State) {
            case GameState.Standard:

                State = newState;
                break;
            case GameState.Challenge:

                State = newState;
                break;

        }
    }
}
