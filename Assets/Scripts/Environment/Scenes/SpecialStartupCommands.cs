using UnityEngine;
using System.Collections;

/// <summary>
/// Controls any special actions that should be taken on startup, usually for debugging purposes.
/// This includes things like "immediately start in this scene from this spawn point."
/// GameManager.Start runs the commands.
/// </summary>
public class SpecialStartupCommands : MonoBehaviour {

    private enum StartupScene { None, Sandbox };

    [SerializeField]
    private bool UseSpecialStartup = false;

    [SerializeField]
    private StartupScene startupScene = StartupScene.None;
    [SerializeField]
    private Actor.ActorType startupActor = Actor.ActorType.Prima;

    /// <summary>
    /// Runs the startup commands.
    /// Returns true if the game starts on the title screen.
    /// </summary>
    public bool RunCommands() {

        bool ret = true;
        int scene = -1;
        if (UseSpecialStartup) {
            switch(startupScene) {
                case StartupScene.Sandbox:
                    ret = false;
                    scene = SceneSelectMenu.sceneSandbox;
                    break;
                default:
                    break;
            }
            switch (startupActor) {
                case Actor.ActorType.Prima:
                    Player.PlayerInstance.SetActor(Prima.PrimaInstance);
                    break;
                case Actor.ActorType.Kog:
                    Player.PlayerInstance.SetActor(Kog.KogInstance);
                    break;
            }
        }

        if(!ret) {
            GameManager.MenusController.mainMenu.splashScreen.Hide();
            GameManager.SceneTransitionManager.LoadScene(scene, false);
        }
        return ret;
    }
}
