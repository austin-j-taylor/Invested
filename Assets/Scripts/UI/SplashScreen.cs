using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SplashScreen : MonoBehaviour {

    public void Hide() {
        // Hide the spash screen
        GetComponent<Image>().enabled = false;
    }
    public void Show() {
        // Show the spash screen
        GetComponent<Image>().enabled = true;
    }
}
