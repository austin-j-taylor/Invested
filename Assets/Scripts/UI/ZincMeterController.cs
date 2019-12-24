using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZincMeterController : MonoBehaviour {

    private const bool up = true;
    private const bool down = false;

    private const float timeToFade = .0001f;//1;
    private const float highX = -28.25f / 2;
    private const float highY = 40.291f / 2;
    private const float speedFactor = .2f;

    private readonly Vector2 lowTop = new Vector2(-highX, -highY);
    private readonly Vector2 highTop = new Vector2(highX, highY);
    private readonly Vector2 lowBottom = new Vector2(-highX, -highY);
    private readonly Vector2 highBottom = new Vector2(highX, highY);

    public static readonly Color ColorZinc = new Color(0.7568628f, 0.8588235f, 1);

    private Image spikeTop;
    private Image spikeBottom;
    private Image spikeTopFill;
    private Image spikeBottomFill;
    private Image frame;
    private Image fill;
    private CanvasGroup side;
    private Image sideImage;
    private Animator animator;

    private float position;
    private bool direction;
    private float timeLastChanged = -100;

    public bool SideEnabled {
        set {
            side.alpha = value ? .5f : 0;
        }
    }

    // Start is called before the first frame update
    void Awake() {
        frame = transform.Find("frame").GetComponent<Image>();
        fill = frame.transform.Find("fill").GetComponent<Image>();
        spikeTop = transform.Find("spikes/spikeTop").GetComponent<Image>();
        spikeBottom = transform.Find("spikes/spikeBottom").GetComponent<Image>();
        spikeTopFill = spikeTop.transform.Find("fill").GetComponent<Image>();
        spikeBottomFill = spikeBottom.transform.Find("fill").GetComponent<Image>();
        side = HUD.Crosshair.transform.Find("ZincSideMeter/Zinc_spikeSide").GetComponent<CanvasGroup>();
        sideImage = side.transform.Find("fill").GetComponent<Image>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void LateUpdate() {
        if (Player.PlayerZinc.Rate != 0)
            timeLastChanged = Time.time;

        animator.SetBool("IsVisible", Time.time - timeLastChanged < timeToFade);
    }

    public void Clear() {
        side.alpha = 0;
        position = .5f;
        direction = up;
        timeLastChanged = -100;
        animator.SetBool("IsVisible", false);
        animator.Play("ZincMeter_Invisible", animator.GetLayerIndex("Visibility"));
    }

    public void ChangeSpikePosition(float reserve) {
        
        if (direction == up) {
            position += (1 - Mathf.Sqrt(reserve)) * Time.timeScale * speedFactor;
        } else {
            position -= (1 - Mathf.Sqrt(reserve)) * Time.timeScale * speedFactor;
        }

        if (position >= 1) {
            position = 1;
            direction = down;
        } else if (position <= 0) {
            position = 0;
            direction = up;
        }
        Color newColorZinc = ColorZinc;
        newColorZinc.a = 1 - Mathf.Sqrt(reserve);

        spikeTop.rectTransform.localPosition = Vector2.Lerp(lowTop, highTop, position);
        spikeBottom.rectTransform.localPosition = Vector2.Lerp(highBottom, lowBottom, position);
        spikeTopFill.color = newColorZinc;
        spikeBottomFill.color = newColorZinc;
        fill.fillAmount = reserve;
        sideImage.fillAmount = reserve;
    }

}
