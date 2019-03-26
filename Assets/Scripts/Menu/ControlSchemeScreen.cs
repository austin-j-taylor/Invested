using UnityEngine;
/*
 * When the game is opened for the first time, this screen opens to ask the user for their control scheme.
 */
public class ControlSchemeScreen : MonoBehaviour {



    private void Awake() {
        
    }

    public void Open() {
        gameObject.SetActive(true);
    }

    public void Close() {
        FlagsController.ControlSchemeChosen = true;
        gameObject.SetActive(false);
        MainMenu.OpenTitleScreen();
    }

}
