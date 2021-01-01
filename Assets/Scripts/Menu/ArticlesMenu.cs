using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the menu that displays in-world information from the title screen.
/// </summary>
public class ArticlesMenu : Menu {

    private const int index_532 = 0, index_614 = 1;
    [SerializeField]
    private Sprite zincImage = null;
    [SerializeField]
    private Text[] articleTexts = null;

    private Text tooltip;
    private TextMeshProUGUI articleText;
    private GameObject textBackground;
    private Image articleImage;
    private Button backButton;

    void Awake() {
        Transform articles = transform.Find("Articles");
        tooltip = articles.Find("Tooltip").GetComponent<Text>();
        articleImage = transform.Find("Content/Image").GetComponent<Image>();
        backButton = transform.Find("Back").GetComponent<Button>();
        textBackground = transform.Find("TextWindow").gameObject;
        articleText = transform.Find("TextWindow/Template/Viewport/Text").GetComponent<TextMeshProUGUI>();
        for (int i = 0; i < articleTexts.Length; i++)
            articleTexts[i].enabled = false;

        backButton.onClick.AddListener(OnClickedBack);
        textBackground.SetActive(false);
    }

    private void Start() {
        gameObject.SetActive(false);
    }

    public override void Open() {
        base.Open();
        tooltip.text = "";
        MainMenu.FocusOnButton(transform);
    }
    public override void Close() {
        if (IsOpen) {
            base.Close();
            GameManager.MenusController.mainMenu.titleScreen.Open();
        }
    }

    private void OnClickedBack() {
        Close();
    }

    public void OnEnteredZinc() {
        articleImage.sprite = zincImage;
        articleImage.gameObject.SetActive(true);
        textBackground.SetActive(false);
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
    public void OnEnteredDirectives532() {
        articleImage.gameObject.SetActive(false);
        tooltip.text = "";
        textBackground.SetActive(true);

        articleText.text = articleTexts[index_532].text;
    }
    public void OnEnteredDirectives612() {
        articleImage.gameObject.SetActive(false);
        tooltip.text = "";
        textBackground.SetActive(true);

        articleText.text = articleTexts[index_614].text;
    }
}
