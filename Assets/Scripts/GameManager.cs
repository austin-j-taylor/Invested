using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using VolumetricLines;
/*
 * At startup, loads the Title Screen scene
 */
public class GameManager : MonoBehaviour {

    //public static GameManager Instance { get; private set; }

    public static Material TargetHighlightMaterial { get; private set; }
    public static VolumetricLineBehavior MetalLineTemplate { get; private set; }

    // Holds all Magnetics in scene
    public static List<Magnetic> MagneticsInScene { get; private set; }

    public static int IgnorePlayerLayer { get; private set; }

    //void Awake() {
    //    if (Instance != null && Instance != this) {
    //        Destroy(this.gameObject);
    //    } else {
    //        Instance = this;
    //    }
    //}

    void Awake() {
        TargetHighlightMaterial = Resources.Load<Material>("targetHighlightMaterial");
        MetalLineTemplate = Resources.Load<VolumetricLineBehavior>("MetalLineTemplate");
        MagneticsInScene = new List<Magnetic>();
        IgnorePlayerLayer = ~(1 << LayerMask.NameToLayer("Player"));
        SceneManager.sceneLoaded += Clear;
    }

    private void Start() {
        SceneManager.LoadScene(SceneSelectMenu.sceneTitleScreen);
    }

    public static void AddMagnetic(Magnetic magnetic) {
        MagneticsInScene.Add(magnetic);
    }

    private void Clear(Scene scene, LoadSceneMode mode) {
        MagneticsInScene = new List<Magnetic>();
    }
}
