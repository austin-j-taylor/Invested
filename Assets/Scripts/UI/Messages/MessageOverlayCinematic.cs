using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static TextCodes;
using System.Collections;

/// <summary>
/// Cenimatically displays a message near the center of the screen.
/// These messages may fade in, fade out, and fade into another.
/// Also supports passing a List of strings to player sequentially with Next().
/// </summary>
public class MessageOverlayCinematic : MonoBehaviour {

    private const int transitionTime = 1;

    private Animator anim;
    private Text messageText;
    private List<string> currentTextList = null;
    private int textIndex = 0;

    void Awake() {
        anim = GetComponent<Animator>();
        messageText = transform.Find("Message").GetComponent<Text>();
    }

    public void Clear() {
        messageText.text = string.Empty;
    }

    public void SetText(string text) {
        messageText.text = text;
    }

    // Fades newText into messageText on the screen.
    public void FadeIn(string newText) {
        StopAllCoroutines();
        anim.SetBool("IsVisible", true);
        messageText.text = newText;
    }
    // Fades newText into messageText on the screen, waits time seconds, then fades out
    public void FadeInFor(string newText, int time) {
        StopAllCoroutines();
        anim.SetBool("IsVisible", true);
        messageText.text = newText;
        StartCoroutine(WaitForThenFadeOut(time));
    }
    // Displays the next text element of the text list last passes to FadeIn.
    // If the text list runs out of elements, fade the message out.
    public void Next() {
        textIndex++;
        if (currentTextList != null && currentTextList.Count > textIndex) {
            FadeOutInto(currentTextList[textIndex]);
        } else {
            FadeOut();
            currentTextList = null;
        }
    }

    // Fade out over 1-2 seconds
    public void FadeOut() {
        StopAllCoroutines();
        anim.SetBool("IsVisible", false);
    }

    // Fades out, then fades into newText
    public void FadeOutInto(string newText) {
        StopAllCoroutines();
        FadeOut();
        StartCoroutine(WaitThenFadeInto(newText));
    }

    private IEnumerator WaitThenFadeInto(string newText) {
        yield return new WaitForSecondsRealtime(1);
        FadeIn(newText);
    }

    private IEnumerator WaitForThenFadeOut(int time) {
        yield return new WaitForSecondsRealtime(time);
        FadeOut();
    }
}
