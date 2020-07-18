using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the major deletion of game data.
/// </summary>
public class DataManagementMenu : Menu {

    private Button saveButton, timeTrialsButton, confirmButton, closeButton;
    private bool deletingSaves = false;

    void Awake() {
        saveButton = transform.Find("Tooltip/DeleteSaves").GetComponent<Button>();
        timeTrialsButton = transform.Find("Tooltip/DeleteTimeTrials").GetComponent<Button>();
        confirmButton = transform.Find("Tooltip/ConfirmButton").GetComponent<Button>();
        closeButton = transform.Find("CloseButton").GetComponent<Button>();

        saveButton.onClick.AddListener(OnClickSave);
        timeTrialsButton.onClick.AddListener(OnClickTimeTrials);
        confirmButton.onClick.AddListener(OnClickConfirm);
        closeButton.onClick.AddListener(Close);
    }
    
    void Start() {
        base.Close();
    }

    public override void Open() {
        base.Open();
        saveButton.gameObject.SetActive(true);
        timeTrialsButton.gameObject.SetActive(true);
        confirmButton.gameObject.SetActive(false);
        saveButton.GetComponentInChildren<Text>().text = "Delete save data";
        timeTrialsButton.GetComponentInChildren<Text>().text = "Delete time trial data";
        SetTooltip("");
        MainMenu.FocusOnButton(closeButton);
    }
    public override void Close() {
        base.Close();
        GameManager.MenusController.mainMenu.Reset();
    }

    public void SetTooltip(string tip) {
        transform.Find("Tooltip").GetComponent<Text>().text = tip;
    }

    #region OnClicks
    private void OnClickSave() {
        saveButton.gameObject.SetActive(false);
        timeTrialsButton.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(true);
        SetTooltip("Really delete all progression?");
        deletingSaves = true;
        MainMenu.FocusOnButton(confirmButton);
    }
    private void OnClickTimeTrials() {
        saveButton.gameObject.SetActive(false);
        timeTrialsButton.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(true);
        SetTooltip("Really reset all time trial records?");
        deletingSaves = false;
        MainMenu.FocusOnButton(confirmButton);
    }
    private void OnClickConfirm() {
        if (deletingSaves) {
            // DELETE SAVE DATA
            FlagsController.DeleteAllData();
            Environment_TitleScreen.Clear();
            Open();
            saveButton.GetComponentInChildren<Text>().text = "Save data deleted.";
        } else {
            // DELETE TIME TRIAL DATA
            PlayerDataController.DeleteAllData();
            Open();
            timeTrialsButton.GetComponentInChildren<Text>().text = "Time trial data deleted.";
        }
    }
    #endregion
}
