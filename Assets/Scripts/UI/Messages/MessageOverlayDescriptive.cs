using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static TextCodes;

/*
 * Displays a message in the corner of the HUD. Less obtrusive than MessageOverlayCinematic, but can display more information.
 * 
 */
public class MessageOverlayDescriptive : MonoBehaviour {

    public Text HeaderText { get; private set; }
    public Text MessageText { get; private set; }
    
    void Awake() {
        HeaderText = transform.Find("Header").GetComponent<Text>();
        MessageText = transform.Find("Message").GetComponent<Text>();
    }

    public void Clear() {
        HeaderText.text = string.Empty;
        MessageText.text = string.Empty;
    }
}
