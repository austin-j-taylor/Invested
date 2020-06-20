using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static TextCodes;

/// <summary>
/// Displays a large message in on the screen. More obtrusive than MessageOverlayCinematic, but can display more information.
/// </summary>
public class MessageOverlayDescriptive : MonoBehaviour {

    private Text headerText;
    private Text messageText;

    void Awake() {
        headerText = transform.Find("Header").GetComponent<Text>();
        messageText = transform.Find("Message").GetComponent<Text>();
    }

    public void Clear() {
        headerText.text = string.Empty;
        messageText.text = string.Empty;
    }

    public void SetContents(string header, string message) {
        headerText.text = header;
        messageText.text = message;
    }
    public void SetHeader(string text) {
        headerText.text = text;
    }
    public void SetMessage(string text) {
        messageText.text = text;
    }
}
