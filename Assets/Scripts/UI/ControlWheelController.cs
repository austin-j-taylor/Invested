﻿using UnityEngine;
using UnityEngine.UI;
using static TextCodes;

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
    public enum LockedState { Unlocked, LockedFully, LockedToArea, LockedToBubble };

    private Image circle;
    private Image[] spokes;
    private Text textCenter;
    private Text textManual;
    private Text textArea;
    private Text textBubble;
    private Text textCoinshot;


    private Selection highlit; // the selection being hovered over
    private Selection selectedSpoke; // manual, area, etc.
    private Selection selectedCoin; // semi auto, full auto, spray
    private LockedState lockedState;

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
                    switch (highlit) {
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
                            Player.PlayerInstance.CoinThrowingMode = Player.CoinMode.Spray;
                            HUD.ThrowingAmmoMeter.Alert(Player.CoinMode.Spray);
                            break;
                        case Selection.Coin_Full:
                            selectedCoin = highlit;
                            Player.PlayerInstance.CoinThrowingMode = Player.CoinMode.Full;
                            HUD.ThrowingAmmoMeter.Alert(Player.CoinMode.Full);
                            break;
                        case Selection.Coin_Semi:
                            selectedCoin = highlit;
                            Player.PlayerInstance.CoinThrowingMode = Player.CoinMode.Semi;
                            HUD.ThrowingAmmoMeter.Alert(Player.CoinMode.Semi);
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

    void Awake() {
        selectedSpoke = Selection.Manual;
        selectedCoin = Selection.Coin_Semi;
        circle = transform.Find("Selections/Circle").GetComponent<Image>();
        spokes = transform.Find("Selections").GetComponentsInChildren<Image>();
        textCenter = transform.Find("Selections/SpokeCircle/Text/Title").GetComponent<Text>();
        textManual = transform.Find("Selections/Spoke0/Text/Title/Description").GetComponent<Text>();
        textArea = transform.Find("Selections/Spoke1/Text/Title/Description").GetComponent<Text>();
        textBubble = transform.Find("Selections/Spoke2/Text/Title/Description").GetComponent<Text>();
        textCoinshot = transform.Find("Selections/Spoke3/Text/Title/Description").GetComponent<Text>();
        SetLockedState(LockedState.Unlocked);
    }

    public void Clear() {
        circle.fillAmount = 1;
        isOpen = false;
        HUD.HideControlWheel(true);
        highlit = Selection.Cancel;
        SetLockedState(LockedState.Unlocked);
        RefreshSpokes();
    }

    private void LateUpdate() {
        if (!PauseMenu.IsPaused) {

            // If the player hits certain keys, consider that selecting a sector of the control wheel.
            if(Player.CanControl) {
                if(Keybinds.ControlWheelManual()) {
                    SectorManual();
                    isOpen = true;
                    IsOpen = false;
                } else if (Keybinds.ControlWheelArea() && lockedState != LockedState.LockedFully) {
                    SectorArea();
                    isOpen = true;
                    IsOpen = false;
                } else if (Keybinds.ControlWheelBubble() && lockedState != LockedState.LockedFully) {
                    SectorBubble();
                    isOpen = true;
                    IsOpen = false;
                } else if (Keybinds.ControlWheelCoinshot() && lockedState == LockedState.Unlocked) {
                    SectorCoinshot();
                    isOpen = true;
                    IsOpen = false;
                } else if (Keybinds.ControlWheelThrowingMode() && lockedState == LockedState.Unlocked) {
                    switch(Player.PlayerInstance.CoinThrowingMode) {
                        case Player.CoinMode.Semi:
                            SectorCoinFull();
                            break;
                        case Player.CoinMode.Full:
                            SectorCoinSpray();
                            break;
                        case Player.CoinMode.Spray:
                            SectorCoinSemi();
                            break;
                    }
                    isOpen = true;
                    IsOpen = false;
                } else if(Keybinds.ControlWheelDeselectAll()) {
                    SectorDeselectAll();
                    isOpen = true;
                    IsOpen = false;
                }
            }

            // State machine for Control Wheel
            if (IsOpen) {
                if (!Keybinds.ControlWheel() || !Player.CanControl || (lockedState == LockedState.LockedFully)) {
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
                        UpdateText();
                    } else { // outside Cancel circle: depending on the angle, choose a selection
                        if (angle > angleDeselectAll_StopBurning) {
                            SectorDeselectAll();
                        } else if (angle > angleStopBurning_Manual) {
                            SectorStopBurning();
                        } else if (angle > angleManual_Area) {
                            SectorManual();
                        } else if (angle > angleArea_Bubble) {
                            SectorArea();
                        } else if (angle > angleBubble_Coinshot) {
                            SectorBubble();
                        } else if (angle > angleCoinshot_CoinSpray) {
                            SectorCoinshot();
                        } else if (angle > angleCoinSpray_CoinFull) {
                            SectorCoinSpray();
                        } else if (angle > angleCoinFull_CoinSemi) {
                            SectorCoinFull();
                        } else if (angle > angleCoinSemi_DeselectAll) {
                            SectorCoinSemi();
                        } else {
                            SectorDeselectAll();
                        }
                    }
                    // Update spoke images to reflect current highlit option
                    RefreshSpokes();
                }
                if (Keybinds.ControlWheelConfirm()) {
                    IsOpen = false;
                }
            } else {
                if (Keybinds.ControlWheelDown() && Player.CanControl && !(lockedState == LockedState.LockedFully)) {
                    IsOpen = true;
                }
            }
        }
    }

    // Sector commands
    private void SectorDeselectAll() {
        UpdateText();
        highlit = Selection.DeselectAll;
    }
    private void SectorStopBurning() {
        UpdateText();
        highlit = Selection.StopBurning;
    }
    private void SectorManual() {
        UpdateText();
        UpdateManual();
        highlit = Selection.Manual;
    }
    private void SectorArea() {
        UpdateText();
        UpdateArea();
        highlit = Selection.Area;
    }
    private void SectorBubble() {
        UpdateText();
        UpdateBubble();
        highlit = Selection.Bubble;
    }
    private void SectorCoinshot() {
        UpdateText();
        UpdateCoinshot();
        highlit = (lockedState == LockedState.Unlocked) ? Selection.Coinshot : Selection.Cancel;
    }
    private void SectorCoinSpray() {
        UpdateText();
        highlit = (lockedState == LockedState.Unlocked) ? Selection.Coin_Spray : Selection.Cancel;
    }
    private void SectorCoinFull() {
        UpdateText();
        highlit = (lockedState == LockedState.Unlocked) ? Selection.Coin_Full : Selection.Cancel;
    }
    private void SectorCoinSemi() {
        UpdateText();
        highlit = (lockedState == LockedState.Unlocked) ? Selection.Coin_Semi : Selection.Cancel;
    }

    public void UpdateText() {
        textManual.text = _KeyPullAbridged + "/" + _KeyPushAbridged + ": " + Pull_Push + "\non a single target\n\n\n";
        textArea.text = _KeyPullAbridged + "/" + _KeyPushAbridged + ": " + Pull_Push + "\nin a cone in front of you\n\n\n\n";
        textBubble.text = _KeyPullAbridged + "/" + _KeyPushAbridged + ": " + Pull_Push + "\nin a bubble around you\n\n"
        + "The bubble can stay open\nin other modes.\n";
        textCoinshot.text = _KeyPullAbridged + ": throw and " + Push + " " + O_Coin + "\n\n\n\n\n";
        // The active mode gets the verbose text as well
        switch(Player.PlayerIronSteel.Mode) {
            case PlayerPullPushController.ControlMode.Coinshot: // fall through
                textCenter.text = "";
                UpdateCoinshot();
                break;
            case PlayerPullPushController.ControlMode.Manual:
                textCenter.text = "";
                UpdateManual();
                break;
            case PlayerPullPushController.ControlMode.Area:
                textCenter.text = KeyNumberOfTargetsAbridged + ":\n Area radius";
                UpdateArea();
                break;
            case PlayerPullPushController.ControlMode.Bubble:
                textCenter.text = KeyNumberOfTargetsAbridged + ":\nBubble radius";
                UpdateBubble();
                break;
        }
    }
    private void UpdateManual() {
        textManual.text = _KeyPullAbridged + "/" + _KeyPushAbridged + ": " + Pull_Push + "\n"
            + _KeySelectAbridged + "/" + _KeySelectAlternateAbridged + ": Mark target\n"
            + s_Hold_ + KeyMultiMark + ": Mark multiple\n\n";
    }
    private void UpdateArea() {
        textArea.text = _KeyPullAbridged + "/" + _KeyPushAbridged + ": " + Pull_Push + "\n"
        + KeyNumberOfTargets + ":\nsize of cone"
        + "\n\n(Mark like in Manual.)\n";
    }
    private void UpdateBubble() {
        textBubble.text = _KeyPullAbridged + "/" + _KeyPushAbridged + ": " + Pull_Push + "\n"
            + _KeySelectAbridged + "/" + _KeySelectAlternateAbridged + ": toggle bubble\n"
        + KeyNumberOfTargets + ":\nsize of bubble\n\n";
    }
    private void UpdateCoinshot() {
        textCoinshot.text = _KeyPullAbridged + ": throw and " + Push + " " + O_Coin + "\n"
            +" when nothing is " + Marked_pulling + ".\n"
            + s_Hold_ + KeyMultiMark + ": Mark when thrown\n\n\n";
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

    // Sets how many options are available on the control wheel
    public void SetLockedState(LockedState newState) {
        lockedState = newState;

        switch (newState) {
            case LockedState.Unlocked:
                for (int i = 0; i < spokes.Length; i++) {
                    spokes[i].gameObject.SetActive(true);
                }
                break;
            case LockedState.LockedFully:
                for (int i = 0; i < spokes.Length; i++) {
                    spokes[i].gameObject.SetActive(false);
                }
                break;
            case LockedState.LockedToArea:
                for (int i = 0; i < 3; i++) {
                    spokes[i].gameObject.SetActive(true);
                }
                for (int i = 3; i < 8; i++) {
                    spokes[i].gameObject.SetActive(false);
                }
                for (int i = 8; i < spokes.Length; i++) {
                    spokes[i].gameObject.SetActive(true);
                }
                break;
            case LockedState.LockedToBubble:
                for (int i = 0; i < 4; i++) {
                    spokes[i].gameObject.SetActive(true);
                }
                for (int i = 4; i < 8; i++) {
                    spokes[i].gameObject.SetActive(false);
                }
                for (int i = 8; i < spokes.Length; i++) {
                    spokes[i].gameObject.SetActive(true);
                }
                break;
        }
    }
    public bool IsLocked() {
        return lockedState == LockedState.LockedFully;
    }
}
