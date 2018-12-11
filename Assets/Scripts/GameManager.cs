using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using VolumetricLines;
/*
 * At startup, loads the Title Screen scene
 * Also holds all Resources
 */
public class GameManager : MonoBehaviour {

    //public static Material Material_TargetHighlight { get; private set; }
    public static Material Material_Gebaude { get; private set; }
    public static Material Material_Ettmetal_Glowing { get; private set; }
    public static Font Font_Heebo { get; private set; }
    public static VolumetricLineBehavior MetalLineTemplate { get; private set; }

    // Holds all Magnetics in scene
    public static List<Magnetic> MagneticsInScene { get; private set; }

    public static int Layer_IgnoreCamera { get; private set; }

    void Awake() {
        //Material_TargetHighlight = Resources.Load<Material>("Materials/targetHighlightMaterial");
        Material_Gebaude = Resources.Load<Material>("Materials/Gebaude");
        Material_Ettmetal_Glowing = Resources.Load<Material>("Materials/Ettmetal_glowing");
        Font_Heebo = Resources.Load<Font>("Fonts/Heebo-Medium");
        MetalLineTemplate = Resources.Load<VolumetricLineBehavior>("MetalLineTemplate");
        MagneticsInScene = new List<Magnetic>();
        Layer_IgnoreCamera = ~((1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Ignore Camera")));
        //SceneManager.sceneLoaded += Clear;
        SceneManager.sceneUnloaded += Clear;
    }

    private void Start() {
        SceneManager.LoadScene(SceneSelectMenu.sceneTitleScreen);
    }

    public static void AddMagnetic(Magnetic magnetic) {
        MagneticsInScene.Add(magnetic);
    }

    public static void RemoveMagnetic(Magnetic magnetic) {
        MagneticsInScene.Remove(magnetic);
    }

    private void Clear(Scene scene) {
        MagneticsInScene = new List<Magnetic>();
    }
}
