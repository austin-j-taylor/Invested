using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static TextCodes;

/*
 * Manages conversations. Interacts with the console and reads/parses the txt files containing conversation data
 */
public class ConversationManager : MonoBehaviour {

    private readonly string rootFilename = Path.Combine(Application.streamingAssetsPath, "Data" + Path.DirectorySeparatorChar + "Conversations" + Path.DirectorySeparatorChar);

    public enum Speaker { None, Kog, Prima, OtherMachine };
    public struct Phrase {
        public Speaker speaker; // the speaker of this phrase
        public string content; // the parsed/formatted/colored contents of this phrase
    }

    public struct Conversation {
        public string key; // the header/title for the conversation, e.g. "CASUAL_CHAT"
        public string content; // the phrases, etc. in the conversation
        // keep track of questions/branches? or parse that at runtime? it's 2020, parsing that at runtime is fine
    }
    private List<Conversation> sceneConversations;


    void Start() {
        SceneManager.sceneLoaded += OnSceneLoaded;
        sceneConversations = new List<Conversation>();
    }

    public void Clear() {

    }


    // When a scene is loaded, load the conversation data for that scene.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Single) { // Not loading all of the scenes, as it does at startup
            string filename = "";
            switch (scene.buildIndex) {
                case SceneSelectMenu.sceneSandbox:
                    filename = "sandbox";
                    break;
                case SceneSelectMenu.sceneTutorial1:
                    filename = "tutorial1";
                    break;
                default:
                    // no text for this scene
                    break;
            }

            if (filename.Length == 0) { // no text for this scene
                return;
            }

            sceneConversations.Clear(); // empty the old list of conversations
            filename = rootFilename + filename + ".txt"; // get the full filename

            // read through all lines in the conversation file
            string[] allFileLines;
            try {
                allFileLines = File.ReadAllLines(filename);
            } catch (System.Exception e) {
                Debug.LogError("Could not open text file for conversations: " + e.Message);
                return;
            }
            // Parse the full data file into a list of conversations
            try {
                for (int i = 0; i < allFileLines.Length; i++) {
                    // first step: find the conversation key
                    int colon = allFileLines[i].IndexOf(':');
                    if (colon == -1)
                        continue;
                    Conversation newConversation;
                    newConversation.key = allFileLines[i].Substring(0, colon);
                    i++; // next line
                    // Remaining lines are put into the conversation's content, until you reach an ending signifier.
                    // Could be more efficient.
                    StringBuilder builder = new StringBuilder();
                    while(!allFileLines[i].Contains("\\%") && i < allFileLines.Length) {
                        builder.Append(allFileLines[i] + System.Environment.NewLine);
                        i++;
                    }
                    // if we hit an EOF, just end it there. If the last line had an ending signifier, we still want than in the content.
                    if (i >= allFileLines.Length)
                        break;
                    builder.Append(allFileLines[i]);
                    newConversation.content = builder.ToString();
                    sceneConversations.Add(newConversation);
                }
            } catch (System.Exception e) {
                Debug.LogError("Could not parse text file into conversation data: " + e.Message);
                return;
            }
            //foreach(Conversation convo in sceneConversations) {
            //    Debug.Log("key: " + convo.key);
            //    Debug.Log("content: " + convo.content);
            //}
        }
    }

    /*
     * Starts the conversation with the given key/title/header e.g. "CASUAL_CHAT"
     * If a conversation is open, kill the old one.
     */
    public void StartConversation(string key) {
        if (sceneConversations.Count == 0)
            return;

        int conversationIndex = GetIndexOfConversation(key);
        if(conversationIndex == -1) {
            Debug.LogError("Could not find key " + key + " in sceneConversations.");
            return;
        }

        // Print the first phrase of that conversation.
        HUD.ConversationHUDController.Open(sceneConversations[conversationIndex]);
    }


    //public void Continue() {
    //    if (sceneConversations.Count == 0)
    //        return;
    //    phraseIndex++;
    //    //HUD.SpeechBubble.SetText(textStrings[conversationIndex][phraseIndex]);
    //}
    //public void SetPhrase(int index) {
    //    if (sceneConversations.Count == 0)
    //        return;
    //    phraseIndex = index;
    //    //HUD.SpeechBubble.SetText(textStrings[conversationIndex][phraseIndex]);
    //}

    // Gets the index in the sceneConversation array of the conversation with the given key.
    // Returns -1 if it can't be found.
    private int GetIndexOfConversation(string key) {
        // search through all conversations in this scene to start this one
        // Replace with a more efficient algorithm once we start having sufficiently large # of conversations in the scene
        // For now, just iterate through and look for the right name
        for(int i = 0; i < sceneConversations.Count; i++) {
            if(key.Equals(sceneConversations[i].key)) {
                return i;
            }
        }
        return -1;
    }
    
}
