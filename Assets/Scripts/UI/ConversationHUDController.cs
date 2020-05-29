using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using static TextCodes;
using static ConversationManager;
using System.Text;

/*
 * Controls the HUD element for the dialogue window that appears for conversations.
 * Parses conversations for colors, italics, pauses, etc.
 */
public class ConversationHUDController : MonoBehaviour {

    private const float delayPerCharacter = .02f, delayPerPause = .125f;

    // The style of text as it's being parsed
    private enum Style { Clear, Italic, Bold, Colored };
    // Waiting: the element is idle, waiting for the user to advance text
    // Writing: the element is printing out text. The Speaker is speaking.
    private enum State { Waiting, Writing };

    public bool IsOpen { get; private set; }

    private Conversation currentConversation;
    private Text headerText; // contains the speaker's name?
    private Text conversationText;

    private Animator anim;
    private State state;

    // Use this for initialization
    void Awake() {
        headerText = transform.Find("ConversationWindow/HeaderText").GetComponent<Text>();
        conversationText = transform.Find("ConversationWindow/ConversationText").GetComponentInChildren<Text>();
        anim = GetComponent<Animator>();
        state = State.Waiting;
    }

    private void Update() {
        if(!PauseMenu.IsPaused && IsOpen && state == State.Waiting) {
            // Advance the conversation
            if(Keybinds.AdvanceConversation()) {
                state = State.Writing; // Picks back up in the SpeakPhraseHelper coroutine
            }
        }
    }

    public void Clear() {
        Close();
        // immediately close
        anim.Play("Close");
    }

    // Opens the dialogue for a specific purpose.
    public void Open(Conversation convo) {
        StopAllCoroutines(); // kill old conversations

        anim.SetBool("IsOpen", true);
        IsOpen = true;
        currentConversation = convo;

        // Speak the first phrase of the conversation
        SpeakNextPhrase();
    }

    public void Close() {
        StopAllCoroutines();
        anim.SetBool("IsOpen", false);
        IsOpen = false;
        state = State.Waiting;
    }

    // Parses and speaks the next phrase, replacing formatted data in the input with the correct Rich Text color tags and returning it
    private void SpeakNextPhrase() {
        state = State.Writing;
        StartCoroutine(SpeakPhraseHelper());
    }

    private IEnumerator SpeakPhraseHelper() {
        // if character == \Cs then text = MidBlue(text up to \c)

        StringBuilder parsed = new StringBuilder();
        Style currentStyle = Style.Clear;
        VoiceBeeper currentSpeaker = Player.PlayerVoiceBeeper;

        // For each character in the conversation:
        for (int i = 0; i < currentConversation.content.Length; i++) {
            // here: We are at the start of a character or signifier of a character

            if (currentConversation.content[i] == '\\') {
                // a signifier
                i++;
                if (currentConversation.content[i] == '/') {
                    // a speaker
                    i++;
                    switch (currentConversation.content[i]) {
                        case 'n': // the environment/neutral/none
                            headerText.text = "";
                            currentSpeaker = Player.PlayerVoiceBeeper;
                            break;
                        case 's': // the Sphere/Prima
                            headerText.text = Color_Prima("Primer Sphere");
                            currentSpeaker = Player.PlayerVoiceBeeper;
                            break;
                        case 'k': // the KOLOSS
                            headerText.text = Color_Kog("Kog");
                            currentSpeaker = Player.PlayerVoiceBeeper;
                            break;
                        case 'm': // other machines
                            headerText.text = Color_Machines("Machine");
                            currentSpeaker = Player.PlayerVoiceBeeper;
                            break;
                        case '*': // Custom signifier. Need to parse up to the next backslash as the name to use.
                            StringBuilder name = new StringBuilder();
                            i++;
                            while(currentConversation.content[i] != '\\') {
                                name.Append(currentConversation.content[i]);
                                i++;
                            }
                            headerText.text = Color_Machines(name.ToString());
                            currentSpeaker = Player.PlayerVoiceBeeper;
                            break;
                        case '?': // ???
                            headerText.text = Color_Prima("???");
                            currentSpeaker = Player.PlayerVoiceBeeper;
                            break;
                        default:
                            Debug.LogError("Failed to parse convesation text in " + currentConversation.key + ": " + currentConversation.content + ": invalid speaker");
                            break;
                    }
                } else {
                    // a non-speaker signifier
                    switch (currentConversation.content[i]) {
                        case 'c':
                            // Remove previous text effects, like bold, italics, and color.
                            // Depending on the current style, remove that current style by appending a closing tag to it.
                            switch (currentStyle) {
                                case Style.Clear:
                                    // do nothing
                                    break;
                                case Style.Italic:
                                    parsed.Append("</i>");
                                    break;
                                case Style.Bold:
                                    parsed.Append("</b>");
                                    break;
                                case Style.Colored:
                                    parsed.Append("</color>");
                                    break;
                            }
                            currentStyle = Style.Clear;
                            break;
                        case 'b':
                            // bold
                            parsed.Append("<b>");
                            currentStyle = Style.Bold;
                            break;
                        case 'i':
                            // italic
                            parsed.Append("<i>");

                            currentStyle = Style.Italic;
                            break;
                        case 'n':
                            // a newline
                            parsed.Append(System.Environment.NewLine);
                            break;
                        case 'p':
                            // Pause. Wait for a time equal to 1/8 * X, where X is [1,9]
                            // If no number follows, just pause for 8 by default.
                            i++;
                            int time = (int)Char.GetNumericValue(currentConversation.content[i]);
                            if (time == -1) {// no number, use 8
                                time = 8;
                                i--;
                            } else if (time < 0 || time > 9) {
                                Debug.LogError("Invalid pause time in " + currentConversation.key + ": " + currentConversation.content + ": " + time);
                            }
                            yield return new WaitForSeconds(delayPerPause * time);
                            break;
                        case 'L':
                            // color for LOCATIONS
                            parsed.Append(Color_Location_Open());
                            currentStyle = Style.Colored;
                            break;
                        case 'S':
                            // color for RED (PUSHES)
                            parsed.Append(Color_Push_Open());
                            currentStyle = Style.Colored;
                            break;
                        case 'U':
                            // color for BLUE (PULLS)
                            parsed.Append(Color_Pull_Open());
                            currentStyle = Style.Colored;
                            break;
                        case 'W':
                            // color for OFF-WHITe (PEWTER)
                            parsed.Append(Color_Pewter_Open());
                            currentStyle = Style.Colored;
                            break;
                        case 'C':
                            // Another name. The next character determines whose name.
                            i++;
                            switch (currentConversation.content[i]) {
                                case 's': // the Sphere/Prima
                                    parsed.Append(Color_Prima_Open());
                                    break;
                                case 'k': // the KOLOSS
                                    parsed.Append(Color_Kog_Open());
                                    break;
                                case 'm': // other machines
                                    parsed.Append(Color_Machines_Open());
                                    break;
                                default:
                                    Debug.LogError("Failed to parse convesation text in " + currentConversation.key + ": " + currentConversation.content + ": invalid character name");
                                    break;
                            }
                            currentStyle = Style.Colored;
                            break;
                        case '%':
                            // Ending signifier. End the conversation, once the player advances one last time.
                            i = currentConversation.content.Length;
                            state = State.Waiting;
                            break;
                        default:
                            Debug.LogError("Failed to parse convesation text in " + currentConversation.key + ": " + currentConversation.content + ": invalid signifier: " + currentConversation.content[i]);
                            break;
                    }
                }
            } else {
                if (currentConversation.content[i] == '\t') {
                    // discard tab characters
                    continue;
                } else if (currentConversation.content[i] == '\n') {
                    // system newline. End the phrase. Wait until user advances text.
                    state = State.Waiting;
                } else {
                    // just a character; say it
                    parsed.Append(currentConversation.content[i]);
                    // and the speaker beeps, skipping some characters

                    char character = currentConversation.content[i];
                    if (state == State.Writing && character != '(' && character != ')' && character != '.' && character != '\n' && character != '\r') {
                        currentSpeaker.Beep();
                    }
                }
            }
            // Finished this pass. Write content to the text object.
            // If the style isn't currently Clear, we need to add a closing tag for the current style.
            switch (currentStyle) {
                case Style.Clear:
                    // do nothing
                    conversationText.text = parsed.ToString();
                    break;
                case Style.Italic:
                    conversationText.text = parsed.ToString() + "</i>";
                    break;
                case Style.Bold:
                    conversationText.text = parsed.ToString() + "</b>";
                    break;
                case Style.Colored:
                    conversationText.text = parsed.ToString() + "</color>";
                    break;
            }
            // Pause. Some characters wait longer.
            if(state == State.Writing) {
                char character = currentConversation.content[i];
                // Don't pause if we're at punctuation, unless we're right before the end of the message
                // (a close parenthesis also indicates the end of a thought, so skip if that's up next)
                if ((character == '.' || character == '?' || character == '!')
                        && !(i+1 < currentConversation.content.Length && (currentConversation.content[i+1] == '(') || currentConversation.content[i+1] == ')')) {
                    yield return new WaitForSeconds(delayPerPause * 3);
                } else {
                    yield return new WaitForSeconds(delayPerCharacter);
                }
            }

            // If the phrase just ended, wait until the user advances the text.
            if (state == State.Waiting) {
                while (state == State.Waiting) {
                    yield return null;
                }
                // wipe the text on the screen
                parsed.Clear();
            }
        }
        // Only reach here if the EOF was reached without an ending signifier. Act like there was an ending signifier.
        EndConversation();
    }
    private void EndConversation() {
        Close();
    }
}