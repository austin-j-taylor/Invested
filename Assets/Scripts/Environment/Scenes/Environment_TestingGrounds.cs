using UnityEngine;
using System.Collections;

public class Environment_TestingGrounds : MonoBehaviour {

    void Start() {
        GameManager.ConversationManager.StartConversation("CASUAL_CHAT");
    }
    void Update() {
        if(!HUD.ConversationHUDController.IsOpen) {
            if(Keybinds.AdvanceConversation()) {
                GameManager.ConversationManager.StartConversation("STILL_HERE");
            }
        }
    }
}
