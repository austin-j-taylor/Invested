using UnityEngine;
using UnityEngine.UI;
using static TextCodes;
using TMPro;

/// <summary>
/// Controls the HUD element (and all logic) for the Control Wheel, which is used for various control modes
/// </summary>
public class ControlWheelController : MonoBehaviour {

    #region constants
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
    #endregion

    enum Selection { Cancel, Manual, Area, Bubble, Coinshot, Coin_Spray, Coin_Full, Coin_Semi, DeselectAll, StopBurning };
    private enum LockedState { Unlocked, LockedFully, LockedToArea, LockedToBubble };

    private Image circle;
    private Image[] spokes;
    private TextMeshProUGUI[] keys;
    private TextMeshProUGUI textCenter;
    private TextMeshProUGUI textManual;
    private TextMeshProUGUI textArea;
    private TextMeshProUGUI textBubble;
    private TextMeshProUGUI textCoinshot;

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
                    // Opening the Control Wheel
                    HUD.ShowControlWheel();
                    circle.fillAmount = (float)Player.PlayerZinc.Reserve;
                } else {
                    // Close the control wheel and confirm that selection
                    ConfirmSelection();
                }
                isOpen = value;
            }
        }
    }
    public bool IsLocked() => lockedState == LockedState.LockedFully;

    void Awake() {
        selectedSpoke = Selection.Manual;
        selectedCoin = Selection.Coin_Semi;
        circle = transform.Find("Selections/Circle").GetComponent<Image>();
        spokes = transform.Find("Selections/Spokes").GetComponentsInChildren<Image>();
        textCenter = transform.Find("Selections/Spokes/SpokeCircle/Text/Title").GetComponent<TextMeshProUGUI>();
        textManual = transform.Find("Selections/Spokes/Spoke0/Text/Title/Description").GetComponent<TextMeshProUGUI>();
        textArea = transform.Find("Selections/Spokes/Spoke1/Text/Title/Description").GetComponent<TextMeshProUGUI>();
        textBubble = transform.Find("Selections/Spokes/Spoke2/Text/Title/Description").GetComponent<TextMeshProUGUI>();
        textCoinshot = transform.Find("Selections/Spokes/Spoke3/Text/Title/Description").GetComponent<TextMeshProUGUI>();
        keys = new TextMeshProUGUI[] {
            transform.Find("Selections/Spokes/Spoke0/Text/Title/Key").GetComponent<TextMeshProUGUI>(),
            transform.Find("Selections/Spokes/Spoke1/Text/Title/Key").GetComponent<TextMeshProUGUI>(),
            transform.Find("Selections/Spokes/Spoke2/Text/Title/Key").GetComponent<TextMeshProUGUI>(),
            transform.Find("Selections/Spokes/Spoke3/Text/Title/Key").GetComponent<TextMeshProUGUI>(),
            transform.Find("Selections/Spokes/Spoke4b/Text/Key").GetComponent<TextMeshProUGUI>(),
            transform.Find("Selections/Spokes/Spoke5_6/Text/Title/Key").GetComponent<TextMeshProUGUI>(),
            transform.Find("Selections/Spokes/Spoke7/Text/Title/Key").GetComponent<TextMeshProUGUI>(),
        };
    }

    void Start() {
        RefreshLocked(); // query Flags for how unlocked the wheel should be
    }

    public void Clear() {
        circle.fillAmount = 1;
        isOpen = false;
        HUD.HideControlWheel(true);
        highlit = Selection.Cancel;
        RefreshSpokes();
    }

    /// <summary>
    /// Check for player keypresses for shortcuts to clicking spokes
    /// </summary>
    private void LateUpdate() {
        if (!PauseMenu.IsPaused) {

            // If the player hits certain keys, consider that selecting a sector of the control wheel.
            if (Player.CanControl) {
                if (Keybinds.ControlWheelManual()) {
                    SectorManual();
                    ConfirmSelection();
                } else if (Keybinds.ControlWheelArea() && lockedState != LockedState.LockedFully) {
                    SectorArea();
                    ConfirmSelection();
                } else if (Keybinds.ControlWheelBubble() && (lockedState == LockedState.LockedToBubble || lockedState == LockedState.Unlocked)) { // bad logic
                    SectorBubble();
                    ConfirmSelection();
                } else if (Keybinds.ControlWheelCoinshot() && lockedState == LockedState.Unlocked) {
                    SectorCoinshot();
                    ConfirmSelection();
                } else if (Keybinds.ControlWheelThrowingMode() && lockedState == LockedState.Unlocked) {
                    switch (Player.PlayerInstance.CoinThrowingMode) {
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
                    ConfirmSelection();
                } else if (Keybinds.ControlWheelDeselectAll()) {
                    SectorDeselectAll();
                    ConfirmSelection();
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
                    if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) {
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
                        RefreshText();
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

    #region sectorManagement
    /// <summary>
    /// Confirm and execute the selected spoke.
    /// </summary>
    private void ConfirmSelection() {
        HUD.HideControlWheel();

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

    // Sector commands
    private void SectorDeselectAll() {
        RefreshText();
        highlit = Selection.DeselectAll;
    }
    private void SectorStopBurning() {
        RefreshText();
        highlit = Selection.StopBurning;
    }
    private void SectorManual() {
        RefreshText();
        RefreshManual();
        highlit = Selection.Manual;
    }
    private void SectorArea() {
        RefreshText();
        RefreshArea();
        highlit = (lockedState != LockedState.LockedFully) ? Selection.Area : Selection.Cancel;
    }
    private void SectorBubble() {
        RefreshText();
        RefreshBubble();
        highlit = (lockedState == LockedState.LockedToBubble || lockedState == LockedState.Unlocked) ? Selection.Bubble : Selection.Cancel;
    }
    private void SectorCoinshot() {
        RefreshText();
        RefreshCoinshot();
        highlit = (lockedState == LockedState.Unlocked) ? Selection.Coinshot : Selection.Cancel;
    }
    private void SectorCoinSpray() {
        RefreshText();
        highlit = (lockedState == LockedState.Unlocked) ? Selection.Coin_Spray : Selection.Cancel;
    }
    private void SectorCoinFull() {
        RefreshText();
        highlit = (lockedState == LockedState.Unlocked) ? Selection.Coin_Full : Selection.Cancel;
    }
    private void SectorCoinSemi() {
        RefreshText();
        highlit = (lockedState == LockedState.Unlocked) ? Selection.Coin_Semi : Selection.Cancel;
    }
    #endregion

    #region refreshing
    public void RefreshText() {
        if (Player.PlayerIronSteel.SteelReserve.IsEnabled) {
            textManual.text = KeyPullPushAbridged + ": " + Pull_Push + "\non a single target\n\n\n";
            textArea.text = KeyPullPushAbridged + ": " + Pull_Push + "\nin an area in front of you\n\n\n";
            textBubble.text = KeyPullPushAbridged + ": " + Pull_Push + "\nin a bubble around you\n\n"
            + "The bubble can stay open\nin other modes.\n";
        } else {
            textManual.text = KeyPullAbridged + ": " + Pull + "\non a single target\n\n\n";
            textArea.text = KeyPullAbridged + ": " + Pull + "\nin an area in front of you\n\n\n";
            textBubble.text = KeyPullAbridged + ": " + Pull + "\nin a bubble around you\n\n"
            + "The bubble can stay open\nin other modes.\n";
        }
        textCoinshot.text = KeyPullAbridged + ": throw and " + Push + " " + O_Coin + "\n\n\n\n\n";
        // The active mode gets the verbose text as well
        switch (Player.PlayerIronSteel.Mode) {
            case PlayerPullPushController.ControlMode.Coinshot: // fall through
                textCenter.text = "";
                RefreshCoinshot();
                break;
            case PlayerPullPushController.ControlMode.Manual:
                textCenter.text = "";
                RefreshManual();
                break;
            case PlayerPullPushController.ControlMode.Area:
                textCenter.text = KeyRadiusAbridged + ":\n Area radius";
                RefreshArea();
                break;
            case PlayerPullPushController.ControlMode.Bubble:
                textCenter.text = KeyRadiusAbridged + ":\nBubble radius";
                RefreshBubble();
                break;
        }
    }
    private void RefreshManual() {
        if (Player.PlayerIronSteel.SteelReserve.IsEnabled) {
            textManual.text = KeyPullPushAbridged + ": " + Pull_Push + "\n"
                + KeyMark_PullPushAbridged + ": Mark target\n"
                + HowToMultiMark + ":" + "\nMark multiple\n";
        } else {
            textManual.text = KeyPullAbridged + ": " + Pull + "\n"
                + KeyMark_PullAbridged + ": Mark target\n"
                + HowToMultiMark + ":" +  "\nMark multiple\n";
        }
    }
    private void RefreshArea() {
        if (Player.PlayerIronSteel.SteelReserve.IsEnabled) {
            textArea.text = KeyPullPushAbridged + ": " + Pull_Push + "\n"
                    + KeyMark_PullPushAbridged + ": Mark targets\n"
                    + KeyRadiusAbridged + ": size of area\n\n\n";
        } else {
            textArea.text = KeyPullAbridged + ": " + Pull + "\n"
                    + KeyMark_PullAbridged + ": Mark targets\n"
                    + KeyRadiusAbridged + ": size of area\n\n\n";
        }
    }
    private void RefreshBubble() {
        // assume we'll never have bubble without Pushing
        textBubble.text = KeyPullPushAbridged + ": " + Pull_Push + "\n"
                + KeyMark_PullPushAbridged + ": toggle bubble\n"
                + KeyRadiusAbridged + ":size of bubble\n\n\n";
    }
    private void RefreshCoinshot() {
        textCoinshot.text = KeyPullAbridged + ": throw and " + Push + " " + O_Coin + "\n"
                + " when nothing is " + Marked_pulling + ".\n"
            + "\n\n\n";
        //+ HowToMultiMark + ": " + Mark + " when thrown\n\n\n";
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

    // Sets how many options are available on the control wheel, depending on what Flags are met
    public void RefreshLocked() {
        if (FlagsController.GetData("pwr_coins")) {
            lockedState = LockedState.Unlocked;
            for (int i = 0; i < spokes.Length; i++) {
                spokes[i].gameObject.SetActive(true);
            }
        } else if (FlagsController.GetData("wheel_bubble")) {
            lockedState = LockedState.LockedToBubble;
            for (int i = 0; i < 4; i++) {
                spokes[i].gameObject.SetActive(true);
            }
            for (int i = 4; i < 8; i++) {
                spokes[i].gameObject.SetActive(false);
            }
            for (int i = 8; i < spokes.Length; i++) {
                spokes[i].gameObject.SetActive(true);
            }
        } else if (FlagsController.GetData("wheel_area")) {
            lockedState = LockedState.LockedToArea;
            for (int i = 0; i < 3; i++) {
                spokes[i].gameObject.SetActive(true);
            }
            for (int i = 3; i < 8; i++) {
                spokes[i].gameObject.SetActive(false);
            }
            for (int i = 8; i < spokes.Length; i++) {
                spokes[i].gameObject.SetActive(true);
            }
        } else {
            lockedState = LockedState.LockedFully;
            for (int i = 0; i < spokes.Length; i++) {
                spokes[i].gameObject.SetActive(false);
            }
        }
    }
    /// <summary>
    /// Sets the visibility of the 1/2/3/4/C/X/Z hotkeys on the wheel.
    /// </summary>
    public void RefreshHotkeys() {
        keys[0].SetText(KeyManual);
        keys[1].SetText(KeyArea);
        keys[2].SetText(KeyBubble);
        keys[3].SetText(KeyCoinshot);
        keys[4].SetText(KeyThrowingMode);
        keys[5].SetText(KeyDeselectAll);
        keys[6].SetText(KeyStopBurning);
    }
    #endregion
}
