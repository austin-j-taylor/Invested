using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/*
 * Controls the HUD element for the "Console" which the player uses to interact with Interfaceable circuit elements.
 */
public class ConsoleController : MonoBehaviour {

    private Image header;
    private Text headerText;
    private Text consoleText;

    private Animator anim;

    // Use this for initialization
    void Awake() {
        header = transform.Find("ConsoleHeader").GetComponent<Image>();
        headerText = header.transform.Find("HeaderText").GetComponent<Text>();
        consoleText = headerText.transform.Find("ConsoleText").GetComponentInChildren<Text>();
        anim = GetComponent<Animator>();
    }

    public void Clear() {
        Close();
    }

    // Opens the console for a specific purpose.
    public void Open() {
        //Player.CanControl = false;
        anim.SetBool("IsOpen", true);
    }

    public void Close() {
        //Player.CanControl = true;
        anim.SetBool("IsOpen", false);
    }
}
