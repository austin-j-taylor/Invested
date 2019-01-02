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

    //public static Material Material_TargetHighlight { get; private set; }
    //public static Material Material_Gebaude { get; private set; }
    public static Material Material_Ettmetal_Glowing { get; private set; }
    public static Font Font_Heebo { get; private set; }
    public static VolumetricLineBehavior MetalLineTemplate { get; private set; }
    public static VolumetricLineStripBehavior MetalLineStripTemplate { get; private set; }

    // Holds all Magnetics and Allomancers in scene
    public static List<AllomanticIronSteel> Allomancers { get; private set; }
    public static List<Magnetic> MagneticsInScene { get; private set; }

    public static int Layer_IgnoreCamera { get; private set; }
    public static int Layer_IgnoreCameraVertically { get; private set; }
    public static int Layer_BlueLines { get; private set; }
    public static int Layer_BlueLinesVisible { get; private set; }

    public static List<string>[] TriggerBeadMessages { get; private set; }

    void Awake() {
        //Material_TargetHighlight = Resources.Load<Material>("Materials/targetHighlightMaterial");
        //Material_Gebaude = Resources.Load<Material>("Materials/Gebaude");
        Material_Ettmetal_Glowing = Resources.Load<Material>("Materials/Ettmetal_glowing");
        Font_Heebo = Resources.Load<Font>("Fonts/Heebo-Medium");
        MetalLineTemplate = Resources.Load<VolumetricLineBehavior>("MetalLineTemplate");
        MetalLineStripTemplate = Resources.Load<VolumetricLineStripBehavior>("MetalLineStripTemplate");
        Allomancers = new List<AllomanticIronSteel>();
        MagneticsInScene = new List<Magnetic>();
        Layer_IgnoreCamera = ~((1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Ignore Camera")) | (1 << LayerMask.NameToLayer("Ignore Player")));
        Layer_IgnoreCameraVertically = ~((1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Ignore Camera Vertically")) | (1 << LayerMask.NameToLayer("Ignore Player")));
        Layer_BlueLines = LayerMask.NameToLayer("Blue Lines");
        Layer_BlueLinesVisible = LayerMask.NameToLayer("Blue Lines Visible");
        //SceneManager.sceneLoaded += Clear;
        SceneManager.sceneUnloaded += Clear;
    }

    private void Start() {
        // TriggerBeadPopup strings
        TriggerBeadMessages = new List<string>[4];
        TriggerBeadMessages[0] = new List<string> {
            TextCodes.KeyPull + " to " + TextCodes.Pull + ".",
            TextCodes.KeyPush + " to " + TextCodes.Push + "."
        };
        TriggerBeadMessages[1] = new List<string> {
            TextCodes.KeyLook + " to look around.\n\n" + TextCodes.KeyMove + " to move.",
            TextCodes.KeyJump + " to jump.\n\n\n\tCollect that small bead up over the ledge.",

        };
        TriggerBeadMessages[2] = new List<string> {
            TextCodes.KeyStartBurning + "\n\tto start burning " + TextCodes.Iron + " or " + TextCodes.Steel + ".",
            TextCodes.s_Press_ + TextCodes.KeySelect + "\n\tto select a metal to be a " + TextCodes.Pull_target + ".\n" +
                TextCodes.s_Press_ + TextCodes.KeySelectAlternate + "\n\tto select a metal to be a " + TextCodes.Push_target + ".",
            TextCodes.KeyPull + " to " + TextCodes.Pull + ".\n" +
                TextCodes.KeyPush + " to " + TextCodes.Push + "."
        };
        TriggerBeadMessages[3] = new List<string> {
            "Vier " + TextCodes.Pull,
            "Funf " + TextCodes.WASD,
            "Sechs "  + TextCodes.KeyCoinshotMode
        };

        SceneManager.LoadScene(SceneSelectMenu.sceneTitleScreen);
    }

    public static void AddAllomancer(AllomanticIronSteel allomancer) {
        Allomancers.Add(allomancer);
    }

    public static void RemoveAllomancer(AllomanticIronSteel allomancer) {
        Allomancers.Remove(allomancer);
    }

    public static void AddMagnetic(Magnetic magnetic) {
        MagneticsInScene.Add(magnetic);
    }

    public static void RemoveMagnetic(Magnetic magnetic) {
        // Remove from all allomancers
        foreach (AllomanticIronSteel allomancer in Allomancers) {
            allomancer.RemoveTarget(magnetic);
        }
        MagneticsInScene.Remove(magnetic);
    }

    private void Clear(Scene scene) {
        MagneticsInScene = new List<Magnetic>();
    }
}
