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
    private Text[] pullTargetsMass;
    private Text[] pushTargetsMass;
    private Text[] pullTargetsSumForce;
    private Text[] pushTargetsSumForce;
    private Text[] pullTargetsActualForce;
    private Text[] pushTargetsActualForce;

    //private readonly Color blue = new Color(0, .4392f, 1, 1);
    //private readonly Color red = new Color(1, 0, 0, 1);

    // Use this for initialization
    void Awake () {
        playerIronSteel = GameObject.FindGameObjectWithTag("Player").GetComponent<AllomanticIronSteel>();
        pullTargetsMass = new Text[AllomanticIronSteel.maxNumberOfTargets];
        pushTargetsMass = new Text[AllomanticIronSteel.maxNumberOfTargets];
        pullTargetsSumForce = new Text[AllomanticIronSteel.maxNumberOfTargets];
        pushTargetsSumForce = new Text[AllomanticIronSteel.maxNumberOfTargets];
        pullTargetsActualForce = new Text[AllomanticIronSteel.maxNumberOfTargets];
        pushTargetsActualForce = new Text[AllomanticIronSteel.maxNumberOfTargets];

        for (int i = 0; i < AllomanticIronSteel.maxNumberOfTargets; i++) {
            pullTargetsMass[i] = Instantiate(templateMass, transform, false);
            pushTargetsMass[i] = Instantiate(templateMass, transform, false);
            pullTargetsMass[i].text = "";
            pushTargetsMass[i].text = "";

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
        } else {

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

            Vector3 positionMass = FPVCameraLock.FirstPersonCamera.WorldToScreenPoint(target.transform.position + heightToTop) + new Vector3(0, massY);
            Vector3 positionActualForce = FPVCameraLock.FirstPersonCamera.WorldToScreenPoint(target.transform.position - heightToTop) + new Vector3(0, actfY);
            //Vector3 positionSumForce = FPVCameraLock.FirstPersonCamera.WorldToScreenPoint(target.transform.position - heightToTop) + new Vector3(0, actfY + sumfY);

            if (positionMass.z > 0) {
                pullTargetsMass[i].transform.position = positionMass;
                //pullTargetsSumForce[i].transform.position = positionSumForce;
                pullTargetsActualForce[i].transform.position = positionActualForce;
                pullTargetsSumForce[i].text = HUD.AllomanticSumString(target.LastAllomanticForce, target.LastAllomanticNormalForceFromAllomancer, true);
                pullTargetsActualForce[i].text = HUD.ForceString(target.LastNetAllomanticForceOnTarget.magnitude);
            } else {

            }
        }
        for (int i = 0; i < playerIronSteel.PushCount; i++) {
            Magnetic target = pushTargets[i];

            Vector3 heightToTop = Vector3.zero;
            heightToTop.y = target.ColliderBody.bounds.size.y / 2f;

            Vector3 positionMass = FPVCameraLock.FirstPersonCamera.WorldToScreenPoint(target.transform.position + heightToTop) + new Vector3(0, massY);
            Vector3 positionActualForce = FPVCameraLock.FirstPersonCamera.WorldToScreenPoint(target.transform.position - heightToTop) + new Vector3(0, actfY);
            //Vector3 positionSumForce = FPVCameraLock.FirstPersonCamera.WorldToScreenPoint(target.transform.position - heightToTop) + new Vector3(0, actfY + sumfY);

            if (positionMass.z > 0) {
                pushTargetsMass[i].transform.position = positionMass;
                //pushTargetsSumForce[i].transform.position = positionSumForce;
                pushTargetsActualForce[i].transform.position = positionActualForce;
                pushTargetsSumForce[i].text = HUD.AllomanticSumString(target.LastAllomanticForce, target.LastAllomanticNormalForceFromAllomancer, true);
                pushTargetsActualForce[i].text = HUD.ForceString(target.LastNetAllomanticForceOnTarget.magnitude);
            } else {

            }
        }
    }

    // Update number of targets
    public void HardRefresh() {
        SoftRefresh();
        int i;
        for (i = 0; i < playerIronSteel.PullCount; i++) {
            pullTargetsMass[i].text = pullTargets[i].Mass.ToString() + "kg";
        }
        while (i < AllomanticIronSteel.maxNumberOfTargets) {
            pullTargetsMass[i].text = "";
            pullTargetsSumForce[i].text = "";
            pullTargetsActualForce[i].text = "";
            i++;
        }

        for (i = 0; i < playerIronSteel.PushCount; i++) {
            pushTargetsMass[i].text = pushTargets[i].Mass.ToString() + "kg";
        }
        while (i < AllomanticIronSteel.maxNumberOfTargets) {
            pushTargetsMass[i].text = "";
            pushTargetsSumForce[i].text = "";
            pushTargetsActualForce[i].text = "";
            i++;
        }
    }

    public void Clear() {
        for (int i = 0; i < AllomanticIronSteel.maxNumberOfTargets; i++) {
            pullTargetsMass[i].text = "";
            pushTargetsMass[i].text = "";
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
}
