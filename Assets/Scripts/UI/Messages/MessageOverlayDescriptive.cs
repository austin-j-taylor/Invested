using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static TextCodes;

/*
 * Displays a message in the corner of the HUD. Less obtrusive than MessageOverlayCinematic, but can display more information.
 * 
 */
public class MessageOverlayDescriptive : MonoBehaviour {

    private Text headerText;
    private Text hessageText;
    
    void Awake() {
        headerText = transform.Find("Header").GetComponent<Text>();
        hessageText = transform.Find("Message").GetComponent<Text>();
    }

    public void Clear() {
        headerText.text = string.Empty;
        hessageText.text = string.Empty;
    }

    public void SetHeader(string text) {
        headerText.text = text;
    }
    public void SetMessage(string text) {
        hessageText.text = text;
    }
}
