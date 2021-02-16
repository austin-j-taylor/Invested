using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Text;

/// <summary>
/// Controls the HUD element for the "Console" which the player uses to interact with Interfaceable circuit elements.
/// </summary>
public class ConsoleController : MonoBehaviour {

    private const float typingSpeed = .025f,
                        delayAfterEntry = .5f;

    public bool IsOpen { get; private set; }

    //private TextMeshProUGUI previewText;

    private string lastSpeaker = "";
    private bool justPartitioned = true;
    TextMeshProUGUI contents, previewText;
    StringBuilder builder;
    Scrollbar scrollbar;

    private Animator anim;

    void Awake() {
        previewText = transform.Find("PreviewText").GetComponent<TextMeshProUGUI>();
        contents = transform.Find("Console/TextWindow/Template/Viewport/Contents").GetComponent<TextMeshProUGUI>();
        scrollbar = transform.Find("Console/TextWindow/Template/Scrollbar").GetComponent<Scrollbar>();
        anim = GetComponent<Animator>();

        builder = new StringBuilder();

        contents.text = string.Empty;
    }
    public void Clear() {
        Close();
        // immediately close
        anim.Play("Close");
    }

    // Opens the console for a specific purpose.
    public void Open() {
        anim.SetBool("IsOpen", true);
        IsOpen = true;
        if (SettingsMenu.settingsGameplay.controlScheme != JSONSettings_Gameplay.Gamepad)
            CameraController.UnlockCamera();
    }

    public void Close() {
        //ClearLog();
        previewText.text = string.Empty;
        anim.SetBool("IsOpen", false);
        IsOpen = false;
        if (!GameManager.MenusController.pauseMenu.IsOpen)
            CameraController.LockCamera();
    }

    public void Toggle() {
        if (IsOpen)
            Close();
        else
            Open();
    }

    //public void ClearLog() {
    //    StopAllCoroutines();
    //}

    //// Enters a "Response" that prints out over time
    //public void TypeInLine(string text, Interfaceable interf) {
    //    StartCoroutine(TypeInLineHelper(text, interf));
    //}

    //private IEnumerator TypeInLineHelper(string text, Interfaceable interf) {
    //    for (int i = 0; i < text.Length; i++) {
    //        Log(text[i]);
    //        yield return new WaitForSeconds(typingSpeed);
    //    }
    //    Log(System.Environment.NewLine);
    //    yield return new WaitForSeconds(delayAfterEntry);
    //    interf.ReceivedReply = true;
    //}

    /// <summary>
    /// Logs a line to the Text Log under the given speaker
    /// </summary>
    public void LogLine(string speaker, string line) {
        justPartitioned = false;
        if (speaker != lastSpeaker) {
            if (speaker != "") {
                builder.Append("$ ");
                builder.Append(speaker);
                builder.Append(" > ");
            }
            lastSpeaker = speaker;
        }
        builder.AppendLine(line);
        contents.text = builder.ToString();

        previewText.text = line;
    }
    /// <summary>
    ///  Used to separate parts of the log, like ends of conversations.
    /// </summary>
    public void LogPartition() {
        if (!justPartitioned) {
            builder.AppendLine("/////////////////////////////////////////////////////");
            lastSpeaker = "";
            justPartitioned = true;

            contents.text = builder.ToString();
        }
    }
}