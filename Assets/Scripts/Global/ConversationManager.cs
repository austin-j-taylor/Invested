using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using static TextCodes;

/*
 * Manages conversations. Interacts with the console and reads/parses the txt files containing conversation data
 */
public class ConversationManager : MonoBehaviour {

    private readonly string rootFilename = Path.Combine(Application.streamingAssetsPath, "Data" + Path.DirectorySeparatorChar + "Conversations" + Path.DirectorySeparatorChar);

    struct Conversation {
        int index; // position in the allFileLines array
        string header; // the header/title for the conversation, e.g. "CASUAL_CHAT"
        string contents; // the phrases, etc. in the conversation
        // keep track of questions/branches? or parse that at runtime? it's 2020, parsing that at runtime is fine
    }

    private string[] allFileLines = null;
    private int conversationIndex = 0, phraseIndex = 0;

    void Start() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    // When a scene is loaded, load the conversation data for that scene.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Single) { // Not loading all of the scenes, as it does at startup
            string filename = "";
            switch (scene.buildIndex) {
                case SceneSelectMenu.sceneSandbox:
                    filename = "sandbox";
                    break;
                default:
                    // no text for this scene
                    break;
            }

            if (filename.Length == 0) { // no text for this scene
                allFileLines = null;
                return;
            }

            filename = rootFilename + filename + ".txt";
            Debug.Log("In " + filename + ":");

            allFileLines = File.ReadAllLines(filename);
            foreach( string line in allFileLines) {
                Debug.Log(line);
            }

            // Parse through allFileLines somewhat now, somewhat later...
        }
    }

    public void StartConversation(int index) {
        if (allFileLines == null)
            return;
        conversationIndex = index;
        phraseIndex = 0;
        //HUD.SpeechBubble.SetSpeaker(speakerName);
    }
    public void Continue() {
        if (allFileLines == null)
            return;
        phraseIndex++;
        //HUD.SpeechBubble.SetText(textStrings[conversationIndex][phraseIndex]);
    }
    public void SetPhrase(int index) {
        if (allFileLines == null)
            return;
        phraseIndex = index;
        //HUD.SpeechBubble.SetText(textStrings[conversationIndex][phraseIndex]);
    }

    // Parses the input string, replacing formatted data in the input with the correct color tags and returning it
    private string ParsePhrase(string input) {
        // if character == \Cs then text = MidBlue(text up to \c)
        return input;
    }

}
