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
    private const float angleBubble_Empty = -22.5f;
    private const float angleEmpty_CoinshotSpray = -67.5f;
    private const float angleCoinshotSpray_CoinshotFull = -82.5f;
    private const float angleCoinshotFull_CoinshotSemi = -97.5f;
    private const float angleCoinshotSemi_DeselectAll = -112.5f;

    enum Selection { Cancel, Manual, Area, Bubble, Empty, Coinshot, Coin_Spray, Coin_Full, Coin_Semi, DeselectAll, StopBurning };
    
    private Image circle;

    private static Selection selected;

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
                    Debug.Log(selected);
                    // Execute selected
                    switch(selected) {
                        case Selection.Cancel:
                            break;
                        case Selection.Manual:
                            break;
                        case Selection.Area:
                            break;
                        case Selection.Bubble:
                            break;
                        case Selection.Empty:
                            break;
                        case Selection.Coinshot:
                            Player.PlayerInstance.ToggleCoinshotMode();
                            break;
                        case Selection.Coin_Spray:
                            break;
                        case Selection.Coin_Full:
                            break;
                        case Selection.Coin_Semi:
                            break;
                        case Selection.DeselectAll:
                            Player.PlayerIronSteel.RemoveAllTargets();
                            break;
                        case Selection.StopBurning:
                            Player.PlayerIronSteel.StopBurning();
                            break;
                    }
                }
            }
            isOpen = value;
        }
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
                        selected = Selection.Cancel;
                    } else { // outside Cancel circle: depending on the angle, choose a selection
                        if (angle > angleDeselectAll_StopBurning) {
                            selected = Selection.DeselectAll;
                        } else if (angle > angleStopBurning_Manual) {
                            selected = Selection.StopBurning;
                        } else if (angle > angleManual_Area) {
                            selected = Selection.Manual;
                        } else if (angle > angleArea_Bubble) {
                            selected = Selection.Area;
                        } else if (angle > angleBubble_Empty) {
                            selected = Selection.Bubble;
                        } else if (angle > angleEmpty_CoinshotSpray) {
                            selected = Selection.Empty;
                        } else if (angle > angleCoinshotSemi_DeselectAll) {
                            // could be Coinshot if input is within outer radius; if not, it's Coin_Spray, Full, or Semi
                            if (radius < pixelRadius * 2) {
                                selected = Selection.Coinshot;
                            } else {
                                if (angle > angleCoinshotSpray_CoinshotFull) {
                                    selected = Selection.Coin_Spray;
                                } else if (angle > angleCoinshotFull_CoinshotSemi) {
                                    selected = Selection.Coin_Full;
                                } else {
                                    selected = Selection.Coin_Semi;
                                }
                            }
                        } else {
                            selected = Selection.DeselectAll;
                        }
                    }
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
    
    void Awake() {
        circle = transform.Find("Circle").GetComponent<Image>();
    }

    public void Clear() {
        circle.fillAmount = 1;
        isOpen = false;
        HUD.HideControlWheel(true);
        selected = Selection.Cancel;
    }
}
