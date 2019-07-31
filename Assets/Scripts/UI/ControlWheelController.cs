using UnityEngine;
using UnityEngine.UI;

public class ControlWheelController : MonoBehaviour {

    private CanvasGroup wheelGroup;
    private Image circle;

    public bool IsOpen { get; private set; }

    private void Update() {
        if(Keybinds.ControlWheel()) {
            IsOpen = true;
            CameraController.UnlockCamera();
            wheelGroup.alpha = 1;
            circle.fillAmount = (float)Player.PlayerZinc.Reserve;
        } else {
            IsOpen = false;
            wheelGroup.alpha = 0;
        }
    }

    // Use this for initialization
    void Start() {
        wheelGroup = GetComponent<CanvasGroup>();
        circle = transform.Find("Circle").GetComponent<Image>();
    }

    public void Clear() {
        wheelGroup.alpha = 0;
        circle.fillAmount = 1;
        IsOpen = false;
    }
}
