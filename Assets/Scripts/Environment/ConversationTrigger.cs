using UnityEngine;
using System.Collections;

/*
 * When this is attached to a gameobject with a collider, it will start the Conversation with the given name.
 * Then, the object is destroyed.
 */

[RequireComponent(typeof(Collider))]
public class ConversationTrigger : MonoBehaviour {

    [SerializeField]
    private string conversationName = "";
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger) {
            GameManager.ConversationManager.StartConversation(conversationName);
            GetComponent<Collider>().enabled = false;
        }
    }
}
