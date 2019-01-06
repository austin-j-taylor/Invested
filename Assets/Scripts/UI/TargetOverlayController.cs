using UnityEngine;
using UnityEngine.UI;

/*
 * Controls the HUD elements that follow push- and pull-targets.
 *  These include elements for Mass, Net Force, Allomantic Force, and Anchored Push Boost Force.
 */
public class TargetOverlayController : MonoBehaviour {

    private const float pixelDelta = 20;
    //private const float voxelDelta = .3f;

    [SerializeField]
    private Text templateMass;
    [SerializeField]
    private Text templateActualForce;
    
    private Text[] pullTargetsSumForce;
    private Text[] pushTargetsSumForce;
    private Text[] pullTargetsActualForce;
    private Text[] pushTargetsActualForce;
    private Text highlightedTargetMass;

    // Use this for initialization
    void Awake() {
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
            pullTargetsActualForce[i].color = HUD.weakBlue;
            pushTargetsActualForce[i].color = HUD.weakBlue;

            pullTargetsSumForce[i] = pullTargetsActualForce[i].GetComponentsInChildren<Text>()[1];
            pushTargetsSumForce[i] = pushTargetsActualForce[i].GetComponentsInChildren<Text>()[1];
            pullTargetsSumForce[i].text = "";
            pushTargetsSumForce[i].text = "";
        }
    }

    private void LateUpdate() {
    }

    public void Clear() {
        HardRefresh();
        highlightedTargetMass.text = "";
        SetPullTextColorWeak();
        SetPushTextColorWeak();
    }

    // Update number of targets
    public void HardRefresh() {
        SoftRefresh();
        for (int i = Player.PlayerIronSteel.PullTargets.Count; i < AllomanticIronSteel.maxNumberOfTargets; i++) {
            pullTargetsSumForce[i].text = "";
            pullTargetsActualForce[i].text = "";
        }

        for (int i = Player.PlayerIronSteel.PushTargets.Count; i < AllomanticIronSteel.maxNumberOfTargets; i++) {
            pushTargetsSumForce[i].text = "";
            pushTargetsActualForce[i].text = "";
        }
    }

    // Update forces, positions on screen
    public void SoftRefresh() {
        if (SettingsMenu.settingsData.hudForces == 1) {
            SoftRefreshTargets(Player.PlayerIronSteel.PullTargets, pullTargetsActualForce, pullTargetsSumForce, SettingsMenu.settingsData.forceComplexity == 1);
            SoftRefreshTargets(Player.PlayerIronSteel.PushTargets, pushTargetsActualForce, pushTargetsSumForce, SettingsMenu.settingsData.forceComplexity == 1);
        }
        // If the target is highlighted and on screen, display mass
        if (SettingsMenu.settingsData.hudMasses == 1) {
            if (Player.PlayerIronSteel.HasHighlightedTarget) {
                Vector3 heightToTop = Vector3.zero;
                heightToTop.y = Player.PlayerIronSteel.HighlightedTarget.ColliderBodyBoundsSizeY / 2f;

                highlightedTargetMass.text = HUD.MassString(Player.PlayerIronSteel.HighlightedTarget.MagneticMass);
                highlightedTargetMass.transform.position = CameraController.ActiveCamera.WorldToScreenPoint(Player.PlayerIronSteel.HighlightedTarget.transform.position + heightToTop) + new Vector3(0, pixelDelta);
            } else { // Target is not highlighted or is not on screen, hide mass label
                highlightedTargetMass.text = "";
            }
        }
    }

    // Clear unwanted fields after changing settings
    public void InterfaceRefresh() {
        if (SettingsMenu.settingsData.hudMasses == 0) {
            highlightedTargetMass.text = "";
        }
        if (SettingsMenu.settingsData.forceComplexity == 0 && SettingsMenu.settingsData.hudForces == 1) {
            for (int i = 0; i < Player.PlayerIronSteel.PullTargets.Count; i++) {
                pullTargetsSumForce[i].text = "";
            }
            for (int i = 0; i < Player.PlayerIronSteel.PushTargets.Count; i++) {
                pushTargetsSumForce[i].text = "";
            }
        } else {
            if (SettingsMenu.settingsData.hudForces == 0) {
                for (int i = 0; i < Player.PlayerIronSteel.PullTargets.Count; i++) {
                    pullTargetsSumForce[i].text = "";
                    pullTargetsActualForce[i].text = "";
                }
                for (int i = 0; i < Player.PlayerIronSteel.PushTargets.Count; i++) {
                    pushTargetsSumForce[i].text = "";
                    pushTargetsActualForce[i].text = "";
                }
            }
        }
    }

    private void SoftRefreshTargets(TargetArray targets, Text[] actualForce, Text[] sumForce, bool refreshSum) {
        for (int i = 0; i < targets.Count; i++) {
            Magnetic target = targets[i];

            Vector3 heightToTop = Vector3.zero;
            heightToTop.y = target.ColliderBodyBoundsSizeY / 2f;

            Vector3 positionActualForce = CameraController.ActiveCamera.WorldToScreenPoint(target.transform.position - heightToTop) + new Vector3(0, -pixelDelta);

            if (positionActualForce.z > 0) {
                actualForce[i].transform.position = positionActualForce;
                actualForce[i].text = HUD.ForceString(target.LastNetForceOnTarget.magnitude, target.NetMass);
                if(refreshSum)
                    sumForce[i].text = HUD.AllomanticSumString(target.LastAllomanticForce, target.LastAnchoredPushBoostFromAllomancer, target.NetMass, true);
            } else { // Target is not on screen
                sumForce[i].text = "";
                actualForce[i].text = "";
            }
        }
    }

    public void SetPullTextColorStrong() {
        for (int i = 0; i < AllomanticIronSteel.maxNumberOfTargets; i++) {
            pullTargetsActualForce[i].color = HUD.strongBlue;
        }
    }

    public void SetPushTextColorStrong() {
        for (int i = 0; i < AllomanticIronSteel.maxNumberOfTargets; i++) {
            pushTargetsActualForce[i].color = HUD.strongBlue;
        }
    }

    public void SetPullTextColorWeak() {
        for (int i = 0; i < AllomanticIronSteel.maxNumberOfTargets; i++) {
            pullTargetsActualForce[i].color = HUD.weakBlue;
        }
    }

    public void SetPushTextColorWeak() {
        for (int i = 0; i < AllomanticIronSteel.maxNumberOfTargets; i++) {
            pushTargetsActualForce[i].color = HUD.weakBlue;
        }
    }

    public void RemoveHighlightedTargetLabel() {
        highlightedTargetMass.text = "";
    }
}
