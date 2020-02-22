using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

/*
 * Controls the HUD element for the "Console" which the player uses to interact with Interfaceable circuit elements.
 */
public class ConsoleController : MonoBehaviour {

    private const float typingSpeed = .025f,
                        delayAfterEntry = .5f;

    public bool IsOpen { get; private set; }

    private Text headerTextLeft;
    private Text headerTextRight;
    private Text consoleTextLeft;
    private Text consoleTextRight;

    private Animator anim;

    private int lineCount;

    // Use this for initialization
    void Awake() {
        headerTextLeft = transform.Find("ConsoleHeader/HeaderTextLeft").GetComponent<Text>();
        consoleTextLeft = headerTextLeft.transform.Find("ConsoleTextLeft").GetComponentInChildren<Text>();
        headerTextRight = transform.Find("ConsoleHeader/HeaderTextRight").GetComponent<Text>();
        consoleTextRight = headerTextRight.transform.Find("ConsoleTextRight").GetComponentInChildren<Text>();
        anim = GetComponent<Animator>();
    }
    //private void Update() {
    //    if (Input.GetKey(KeyCode.R))
    //        Open();
    //    else
    //        Close();
    //}
    public void Clear() {
        Close();
    }

    // Opens the console for a specific purpose.
    public void Open() {
        anim.SetBool("IsOpen", true);
        IsOpen = true;
    }

    public void Close() {
        ClearLog();
        anim.SetBool("IsOpen", false);
        IsOpen = false;
    }

    public void ClearLog() {
        StopAllCoroutines();
        lineCount = 0;
        headerTextLeft.text = "Console";
        consoleTextLeft.text = string.Empty;
        RefreshRightText();
    }

    public void Log(string text) {
        consoleTextLeft.text += text;
        if (text.Contains(System.Environment.NewLine))
            lineCount++;
        RefreshRightText();
    }
    public void Log(char character) {
        consoleTextLeft.text += character;
        if (character == System.Environment.NewLine[0]) // rip to your windows but i'm different
            lineCount++;
        RefreshRightText();
    }
    public void LogLine(string text) {
        consoleTextLeft.text += text + System.Environment.NewLine;
        lineCount++;
        RefreshRightText();
    }

    // Enters a "Response" that prints out over time
    public void TypeInLine(string text, Interfaceable interf) {
        StartCoroutine(TypeInLineHelper(text, interf));
    }

    private IEnumerator TypeInLineHelper(string text, Interfaceable interf) {
        for(int i = 0; i < text.Length; i++) {
            Log(text[i]);
            yield return new WaitForSeconds(typingSpeed);
        }
        Log(System.Environment.NewLine);
        yield return new WaitForSeconds(delayAfterEntry);
        interf.ReceivedReply = true;
    }

    private void RefreshRightText() {
        headerTextRight.text = headerTextLeft.text.ToLower();
        consoleTextRight.text = consoleTextLeft.text.ToLower();
    }
}
