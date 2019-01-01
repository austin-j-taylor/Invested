using UnityEngine;
using UnityEngine.UI;

public class MessageOverlayController : MonoBehaviour {

    public Text Text { get; private set; }

    // Use this for initialization
    void Start() {
        Text = GetComponentInChildren<Text>();
    }

    public void Clear() {
        Text.text = "";
    }
}
