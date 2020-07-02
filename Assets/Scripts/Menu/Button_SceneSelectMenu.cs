using UnityEngine;
using System.Collections;
using UnityEngine.UI;
/// <summary>
/// Set the Completed image next to the button when the level is completed
/// </summary>
public class Button_SceneSelectMenu : MonoBehaviour {

    [SerializeField]
    private string flag = null;
    private Image completedImage;

    void Awake() {
        completedImage = transform.Find("Completed").GetComponent<Image>();
    }

    public void CheckCompleted() {
        completedImage.enabled = GetComponent<Button>().interactable && FlagsController.GetData(flag);
    }
}
