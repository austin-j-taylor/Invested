using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ArticlesMenu : MonoBehaviour {
    
    public bool IsOpen {
        get {
            return gameObject.activeSelf;
        }
    }

    private Text tooltip;
    private Button zincButton;
    //private Button article2Button;
    private Button backButton;

    void Start() {
        tooltip = transform.Find("Articles/Tooltip").GetComponent<Text>();
        Button[] buttons = GetComponentsInChildren<Button>();
        zincButton = buttons[0];
        //article2Button = buttons[1];
        backButton = buttons[1];
        
        backButton.onClick.AddListener(OnClickedBack);
        
    }

    public void Open() {
        gameObject.SetActive(true);
        tooltip.text = "";
        MainMenu.FocusOnCurrentMenu(transform);
    }

    public void Close() {
        gameObject.SetActive(false);
        MainMenu.OpenTitleScreen();
    }

    public void OnEnteredZinc() {
        tooltip.text = "FROM: elar.tarc@marl.sil\n" +
            "TO: shen.khol@marl.sil\n" +
            "SUBJECT: Zinc peripheral info leak to coppernet resolved\n\n" +
            "Shen,\n\n" +
            "Everything should be resolved. The copperpedia devs and I had a realmail chain and they deleted the classified information from the zinc peripheral page's history. " +
            "Whoever did it probably wasn't even aware they were leaking classified info. " +
            "I'd bet they noticed the article was a stub and just wanted to add some stuff that they learned at work, not realizing it was sensitive. " +
            "I guess this shows that most security breaches come from within, yeah?\n\n" +
            "I attached a screenshot of the article after the scrubbing. I kept the part about the press release, since that's definitely unclassified.\n\n" +
            "V/R\n\n" +
            "Elarin Tarcsel\n" +
            "Senior Allomechanical Engineer, Software Integration Section\n" +
            "realmail: elar.tarc@marl.sil\n" +
            "copper: xFC64:40AA\n" +
            "spirit: ORCHARD-MAGNESIUM-REED";
    }

    public void OnEnteredArticle2() {
        tooltip.text = "test";
    }

    private void OnClickedBack() {
        Close();
    }
}
