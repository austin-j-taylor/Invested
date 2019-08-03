using UnityEngine;
using UnityEngine.UI;

/*
 * Controls the HUD element (and all logic) for the Control Wheel, which is used for various Push controls
 */
public class ControlWheelController : MonoBehaviour {

    private const float cancelRadius = 0.34033203125f;
    private const float angleDeselectAll_StopBurning = 157.5f;
    private const float angleStopBurning_Manual = 112.5f;
    private const float angleManual_Area = 67.5f;
    private const float angleArea_Bubble = 22.5f;
    private const float angleBubble_Coinshot = -22.5f;
    private const float angleCoinshot_CoinSpray = -67.5f;
    private const float angleCoinSpray_CoinFull = -82.5f;
    private const float angleCoinFull_CoinSemi = -97.5f;
    private const float angleCoinSemi_DeselectAll = -112.5f;

    private readonly Color colorBlankSpoke = new Color(1, 1, 1, .1f);
    private readonly Color colorHighlitSpoke = new Color(1, 1, 1, .25f);
    private readonly Color colorSelectedSpoke = new Color(1, 1, 1, .5f);

    enum Selection { Cancel, Manual, Area, Bubble, Coinshot, Coin_Spray, Coin_Full, Coin_Semi, DeselectAll, StopBurning };

    private Image circle;
    private Image[] spokes;

    private Selection highlit; // the selection being hovered over
    private Selection selectedSpoke; // manual, area, etc.
    private Selection selectedCoin; // semi auto, full auto, spray

    private bool isOpen = false;
    public bool IsOpen {
        get {
            return isOpen;
        }
        private set {
            if (isOpen != value) {
                if (value) {
                    if (SettingsMenu.settingsData.controlScheme != SettingsData.Gamepad)
                        CameraController.UnlockCamera();
                    HUD.ShowControlWheel();
                    circle.fillAmount = (float)Player.PlayerZinc.Reserve;
                } else {
                    if (SettingsMenu.settingsData.controlScheme != SettingsData.Gamepad)
                        CameraController.LockCamera();
                    HUD.HideControlWheel();

                    // Execute selected
                    switch(highlit) {
                        case Selection.Cancel:
                            break;
                        case Selection.Manual:
                            selectedSpoke = highlit;
                            Player.PlayerIronSteel.SetControlModeManual();
                            break;
                        case Selection.Area:
                            selectedSpoke = highlit;
                            Player.PlayerIronSteel.SetControlModeArea();
                            break;
                        case Selection.Bubble:
                            selectedSpoke = highlit;
                            Player.PlayerIronSteel.SetControlModeBubble();
                            break;
                        case Selection.Coinshot:
                            selectedSpoke = highlit;
                            Player.PlayerIronSteel.SetControlModeCoinshot();
                            break;
                        case Selection.Coin_Spray:
                            selectedCoin = highlit;
                            break;
                        case Selection.Coin_Full:
                            selectedCoin = highlit;
                            break;
                        case Selection.Coin_Semi:
                            selectedCoin = highlit;
                            break;
                        case Selection.DeselectAll:
                            Player.PlayerIronSteel.RemoveAllTargets();
                            break;
                        case Selection.StopBurning:
                            Player.PlayerIronSteel.StopBurning();
                            break;
                    }
                    RefreshSpokes();
                }
            }
            isOpen = value;
        }
    }

    void Start() {
        selectedSpoke = Selection.Manual;
        selectedCoin = Selection.Coin_Semi;
        circle = transform.Find("Circle").GetComponent<Image>();
        spokes = transform.Find("Selections").GetComponentsInChildren<Image>();
    }

    public void Clear() {
        circle.fillAmount = 1;
        isOpen = false;
        HUD.HideControlWheel(true);
        highlit = Selection.Cancel;
        RefreshSpokes();
    }

    private void LateUpdate() {
        if (!PauseMenu.IsPaused) {
            // State machine for Control Wheel
            if (IsOpen) {
                if (!Keybinds.ControlWheel() || !Player.CanControlPlayer) {
                    IsOpen = false;
                } else {
                    circle.fillAmount = (float)Player.PlayerZinc.Reserve;

                    // Figure out size of screen; if input is outside of radius, it is selecting something other than Cancel
                    int heightPixels = Screen.height;
                    int widthPixels = Screen.width;
                    int squareSize = ((widthPixels > heightPixels) ? heightPixels : widthPixels) / 2;
                    // Find the size of the Cancel circle in pixels from the center of the screen
                    int pixelRadius = (int)(squareSize * cancelRadius);
                    int radius;
                    float angle;
                    // Gamepad and mouse/keyboard select slightly differently. Gamepad uses left joystick, mouse uses mouse
                    if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) {
                        int pixelsX = (int)(Keybinds.Horizontal() * squareSize);
                        int pixelsY = (int)(Keybinds.Vertical() * squareSize);
                        radius = (int)Mathf.Sqrt(pixelsX * pixelsX + pixelsY * pixelsY);
                        angle = Mathf.Atan2(pixelsY, pixelsX) * Mathf.Rad2Deg;
                    } else {
                        Vector3 mouseFromCenter = Input.mousePosition;
                        mouseFromCenter.x -= widthPixels / 2;
                        mouseFromCenter.y -= heightPixels / 2;
                        radius = (int)Mathf.Sqrt(mouseFromCenter.x * mouseFromCenter.x + mouseFromCenter.y * mouseFromCenter.y);
                        angle = Mathf.Atan2(mouseFromCenter.y, mouseFromCenter.x) * Mathf.Rad2Deg;
                    }
                    // The selection depends on the radius and angle of input
                    if (radius < pixelRadius) {
                        highlit = Selection.Cancel;
                    } else { // outside Cancel circle: depending on the angle, choose a selection
                        if (angle > angleDeselectAll_StopBurning) {
                            highlit = Selection.DeselectAll;
                        } else if (angle > angleStopBurning_Manual) {
                            highlit = Selection.StopBurning;
                        } else if (angle > angleManual_Area) {
                            highlit = Selection.Manual;
                        } else if (angle > angleArea_Bubble) {
                            highlit = Selection.Area;
                        } else if (angle > angleBubble_Coinshot) {
                            highlit = Selection.Bubble;
                        } else if (angle > angleCoinshot_CoinSpray) {
                            highlit = Selection.Coinshot;
                        } else if (angle > angleCoinSpray_CoinFull) {
                            highlit = Selection.Coin_Spray;
                        } else if (angle > angleCoinFull_CoinSemi) {
                            highlit = Selection.Coin_Full;
                        } else if (angle > angleCoinSemi_DeselectAll) {
                            highlit = Selection.Coin_Semi;
                        } else {
                            highlit = Selection.DeselectAll;
                        }
                    }
                    // Update spoke images to reflect current highlit option
                    RefreshSpokes();
                }
                if (Keybinds.ControlWheelConfirm()) {
                    IsOpen = false;
                }
            } else {
                if (Keybinds.ControlWheelDown()) {
                    IsOpen = true;
                }
            }
        }
    }

    // Set the color of all spokes:
    // Selected spoke: dark gray
    // Highlit spoke: light gray
    // Others: invisible
    private void RefreshSpokes() {
        // clear all spokes
        foreach (Image image in spokes) {
            image.color = colorBlankSpoke;
        }

        spokes[(int)selectedSpoke].color = colorSelectedSpoke;
        spokes[(int)selectedCoin].color = colorSelectedSpoke;
        spokes[(int)highlit].color = colorHighlitSpoke;
        
    }
}
