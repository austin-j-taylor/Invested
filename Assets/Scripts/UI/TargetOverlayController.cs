using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetOverlayController : MonoBehaviour {

    [SerializeField]
    private Text templateMass;
    [SerializeField]
    private Text templateActualForce;

    private AllomanticIronSteel playerIronSteel;
    private Magnetic[] pullTargets;
    private Magnetic[] pushTargets;
    private Text[] pullTargetsSumForce;
    private Text[] pushTargetsSumForce;
    private Text[] pullTargetsActualForce;
    private Text[] pushTargetsActualForce;
    private Text highlightedTargetMass;

    //private readonly Color blue = new Color(0, .4392f, 1, 1);
    //private readonly Color red = new Color(1, 0, 0, 1);

    // Use this for initialization
    void Awake () {
        playerIronSteel = GameObject.FindGameObjectWithTag("Player").GetComponent<AllomanticIronSteel>();
        pullTargetsSumForce = new Text[AllomanticIronSteel.maxNumberOfTargets];
        pushTargetsSumForce = new Text[AllomanticIronSteel.maxNumberOfTargets];
        pullTargetsActualForce = new Text[AllomanticIronSteel.maxNumberOfTargets];
        pushTargetsActualForce = new Text[AllomanticIronSteel.maxNumberOfTargets];

        highlightedTargetMass = Instantiate(templateMass, transform, false);
        highlightedTargetMass.text = "";

        for (int i = 0; i < AllomanticIronSteel.maxNumberOfTargets; i++) {
            pullTargetsActualForce[i] = Instantiate(templateActualForce, transform, false);
            pushTargetsActualForce[i] = Instantiate(templateActualForce, transform, false);
            pullTargetsActualForce[i].text = "";
            pushTargetsActualForce[i].text = "";

            pullTargetsSumForce[i] = pullTargetsActualForce[i].GetComponentsInChildren<Text>()[1];
            pushTargetsSumForce[i] = pushTargetsActualForce[i].GetComponentsInChildren<Text>()[1];
            pullTargetsSumForce[i].text = "";
            pushTargetsSumForce[i].text = "";
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (playerIronSteel.IsBurningIronSteel) {
            SoftRefresh();
        }
    }
    private const float massY = 20;
    private const float actfY = -20;
    //private const float sumfY = -25;

    // Update forces, positions on screen
    private void SoftRefresh() {

        for (int i = 0; i < playerIronSteel.PullCount; i++) {
            Magnetic target = pullTargets[i];
            
            Vector3 heightToTop = Vector3.zero;
            heightToTop.y = target.ColliderBody.bounds.size.y / 2f;

            Vector3 positionActualForce = FPVCameraLock.FirstPersonCamera.WorldToScreenPoint(target.transform.position - heightToTop) + new Vector3(0, actfY);
            
            if (positionActualForce.z > 0) {
                //pullTargetsSumForce[i].transform.position = positionSumForce;
                pullTargetsActualForce[i].transform.position = positionActualForce;
                pullTargetsSumForce[i].text = HUD.AllomanticSumString(target.LastAllomanticForce, target.LastAllomanticNormalForceFromAllomancer, true);
                pullTargetsActualForce[i].text = HUD.ForceString(target.LastNetAllomanticForceOnTarget.magnitude);
            } else { // Target is not on screen
                pullTargetsSumForce[i].text = "";
                pullTargetsActualForce[i].text = "";
            }
        }

        for (int i = 0; i < playerIronSteel.PushCount; i++) {
            Magnetic target = pushTargets[i];

            Vector3 heightToTop = Vector3.zero;
            heightToTop.y = target.ColliderBody.bounds.size.y / 2f;
            
            Vector3 positionActualForce = FPVCameraLock.FirstPersonCamera.WorldToScreenPoint(target.transform.position - heightToTop) + new Vector3(0, actfY);
            //Vector3 positionSumForce = FPVCameraLock.FirstPersonCamera.WorldToScreenPoint(target.transform.position - heightToTop) + new Vector3(0, actfY + sumfY);

            if (positionActualForce.z > 0) {
                //pushTargetsSumForce[i].transform.position = positionSumForce;
                pushTargetsActualForce[i].transform.position = positionActualForce;
                pushTargetsSumForce[i].text = HUD.AllomanticSumString(target.LastAllomanticForce, target.LastAllomanticNormalForceFromAllomancer, true);
                pushTargetsActualForce[i].text = HUD.ForceString(target.LastNetAllomanticForceOnTarget.magnitude);
            } else { // Target is not on screen
                pushTargetsSumForce[i].text = "";
                pushTargetsActualForce[i].text = "";
            }
        }

        // If the target is highlighted and on screen, display mass
        if (playerIronSteel.HasHighlightedTarget) {
            Vector3 heightToTop = Vector3.zero;
            heightToTop.y = playerIronSteel.HighlightedTarget.ColliderBody.bounds.size.y / 2f;

            highlightedTargetMass.text = playerIronSteel.HighlightedTarget.Mass.ToString() + "kg";
            highlightedTargetMass.transform.position = FPVCameraLock.FirstPersonCamera.WorldToScreenPoint(playerIronSteel.HighlightedTarget.transform.position + heightToTop) + new Vector3(0, massY);
        } else { // Target is not highlighted or is not on screen, hide mass label
            highlightedTargetMass.text = "";
        }
    }

    // Update number of targets
    public void HardRefresh() {
        SoftRefresh();
        for (int i = playerIronSteel.PullCount; i < AllomanticIronSteel.maxNumberOfTargets; i++) {
            pullTargetsSumForce[i].text = "";
            pullTargetsActualForce[i].text = "";
        }

        for (int i = playerIronSteel.PushCount; i < AllomanticIronSteel.maxNumberOfTargets; i++) {
            pushTargetsSumForce[i].text = "";
            pushTargetsActualForce[i].text = "";
        }
    }

    public void Clear() {
        for (int i = 0; i < AllomanticIronSteel.maxNumberOfTargets; i++) {
            highlightedTargetMass.text = "";
            pullTargetsSumForce[i].text = "";
            pushTargetsSumForce[i].text = "";
            pullTargetsActualForce[i].text = "";
            pushTargetsActualForce[i].text = "";
        }
    }

    public void SetTargets(Magnetic[] pull, Magnetic[] push) {
        pullTargets = pull;
        pushTargets = push;
        HardRefresh();
    }

    public void RemoveHighlightedTargetLabel() {
        highlightedTargetMass.text = "";
    }
}
