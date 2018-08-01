using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetOverlayController : MonoBehaviour {

    [SerializeField]
    private Text template;

    public AllomanticIronSteel playerIronSteel;
    public Magnetic[] pullTargets;
    public Magnetic[] pushTargets;
    public Text[] pullTargetsText;
    public Text[] pushTargetsText;

    // Use this for initialization
    void Awake () {
        playerIronSteel = GameObject.FindGameObjectWithTag("Player").GetComponent<AllomanticIronSteel>();
        pullTargetsText = new Text[AllomanticIronSteel.maxNumberOfTargets];
        pushTargetsText = new Text[AllomanticIronSteel.maxNumberOfTargets];

        for(int i = 0; i < AllomanticIronSteel.maxNumberOfTargets; i++) {
            pullTargetsText[i] = Instantiate(template, transform, false);
            pushTargetsText[i] = Instantiate(template, transform, false);
            pullTargetsText[i].text = "";
            pushTargetsText[i].text = "";
        }
	}
	
	// Update is called once per frame
	void Update () {
        SoftRefresh();
    }

    // Update forces, positions on screen
    private void SoftRefresh() {
        for (int i = 0; i < playerIronSteel.PullCount; i++) {
            Vector3 position = FPVCameraLock.FirstPersonCamera.WorldToScreenPoint(pullTargets[i].transform.position + new Vector3(0, .5f));
            if (position.z > 0) {
                pullTargetsText[i].transform.position = position;
            } else {

            }
        }
        for (int i = 0; i < playerIronSteel.PushCount; i++) {
            Vector3 position = FPVCameraLock.FirstPersonCamera.WorldToScreenPoint(pushTargets[i].transform.position + new Vector3(0, .5f));
            if (position.z > 0) {
                pushTargetsText[i].transform.position = position;
            } else {

            }
        }
    }

    // Update number of targets
    public void HardRefresh() {
        SoftRefresh();
        int i;
        for (i = 0; i < playerIronSteel.PullCount; i++) {
            pullTargetsText[i].text = pullTargets[i].Mass.ToString() + "kg";
        }
        while (i < AllomanticIronSteel.maxNumberOfTargets) {
            pullTargetsText[i].text = "";
            i++;
        }

        for (i = 0; i < playerIronSteel.PushCount; i++) {
            pushTargetsText[i].text = pushTargets[i].Mass.ToString() + "kg";
        }
        while (i < AllomanticIronSteel.maxNumberOfTargets) {
            pushTargetsText[i].text = "";
            i++;
        }

    }

    public void Clear() {
        for (int i = 0; i < AllomanticIronSteel.maxNumberOfTargets; i++) {
            pullTargetsText[i].text = "";
            pushTargetsText[i].text = "";
        }
    }

    public void SetTargets(Magnetic[] pull, Magnetic[] push) {
        pullTargets = pull;
        pushTargets = push;
        HardRefresh();
    }
}
