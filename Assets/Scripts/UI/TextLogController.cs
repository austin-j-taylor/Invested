using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.UI;

/// <summary>
/// Controls the HUD element for the Text Log, which displays the most recent messages from the ConversationHUDController and MessageCinematicController.
/// </summary>
public class TextLogController : MonoBehaviour {

    public bool IsOpen => gameObject.activeSelf;

    private string lastSpeaker = "";
    private bool justPartitioned = true;
    TextMeshProUGUI contents;
    StringBuilder builder;
    Scrollbar scrollbar;

    void Awake() {
        builder = new StringBuilder();
        contents = transform.Find("LogWindow/Template/Viewport/LogText").GetComponent<TextMeshProUGUI>();
        scrollbar = transform.Find("LogWindow/Template/Scrollbar").GetComponent<Scrollbar>();
    }

    public void Clear() {
        Close();
    }

    public void Open() {
        gameObject.SetActive(true);
        if (SettingsMenu.settingsGameplay.controlScheme != JSONSettings_Gameplay.Gamepad)
            CameraController.UnlockCamera();
    }
    public void Close() {
        gameObject.SetActive(false);
        if(!GameManager.MenusController.pauseMenu.IsOpen)
            CameraController.LockCamera();
    }
    public void Toggle() {
        if (IsOpen)
            Close();
        else
            Open();
    }

    /// <summary>
    /// Logs a line to the Text Log under the given speaker
    /// </summary>
    public void LogLine(string speaker, string line) {
        justPartitioned = false;
        if (speaker != lastSpeaker) {
            if (speaker != "") {
                builder.Append(speaker);
                builder.Append(": ");
            }
            lastSpeaker = speaker;
        }
        builder.AppendLine(line);
        contents.text = builder.ToString();
    }
    /// <summary>
    ///  Used to separate parts of the log, like ends of conversations.
    /// </summary>
    public void LogPartition() {
        if (!justPartitioned) {
            builder.AppendLine("-----------------------------------------------------");
            lastSpeaker = "";
            justPartitioned = true;

            contents.text = builder.ToString();
        }
    }
}
