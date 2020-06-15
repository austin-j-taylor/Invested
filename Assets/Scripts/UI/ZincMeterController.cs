using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZincMeterController : MonoBehaviour {

    private const float timeToFade = 1;
    public static readonly Color ColorZinc = new Color(0.7568628f, 0.8588235f, 1);

    private CanvasGroup side;
    private Image sideImage;
    private Animator animator;

    private float position;
    private bool direction;
    private float timeLastChanged = -100;


    // Start is called before the first frame update
    void Awake() {
        side = HUD.Crosshair.transform.Find("ZincSideMeter/Zinc_spikeSide").GetComponent<CanvasGroup>();
        sideImage = side.transform.Find("fill").GetComponent<Image>();
        //animator = GetComponent<Animator>();
        animator = side.GetComponent<Animator>();
    }

    // Update is called once per frame
    void LateUpdate() {
        if (Player.PlayerZinc.Rate != 0)
            timeLastChanged = Time.time;

        animator.SetBool("IsVisible", Time.time - timeLastChanged < timeToFade);

        float percent = (float)Player.PlayerZinc.Reserve;
        Color newColorZinc = ColorZinc;
        newColorZinc.a = 1 - Mathf.Sqrt(percent);

        sideImage.fillAmount = percent;
    }

    public void Clear() {
        side.alpha = 0;
        timeLastChanged = -100;
        animator.SetBool("IsVisible", false);
        animator.Play("ZincMeter_Invisible", animator.GetLayerIndex("Visibility"));
    }
}
