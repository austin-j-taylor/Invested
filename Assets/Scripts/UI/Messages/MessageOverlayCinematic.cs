using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static TextCodes;

/*
 * Displays a message about 1/3 from the top of the screen, in the center.
 * Displays information in a nice-looking way.
 * 
 */
public class MessageOverlayCinematic : MonoBehaviour {

    private Text messageText;

    void Awake() {
        messageText = transform.Find("Message").GetComponent<Text>();
    }

    public void Clear() {
        messageText.text = string.Empty;
    }

    public void FadeIn(string newText) {
        messageText.text = newText;
    }
    // Fade out over 1-2 seconds
    public void FadeOut() {
        messageText.text = string.Empty;
    }
}
