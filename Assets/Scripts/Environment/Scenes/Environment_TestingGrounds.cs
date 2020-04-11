using UnityEngine;
using System.Collections;

public class Environment_TestingGrounds : MonoBehaviour {

    void Start() {
        GameManager.ConversationManager.StartConversation("CASUAL_CHAT");
    }

}
