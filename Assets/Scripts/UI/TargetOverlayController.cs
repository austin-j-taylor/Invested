using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the HUD elements that appear over marked targets.
/// These include elements for Mass, Net Force, Allomantic Force, and Anchored Push Boost Force.
/// </summary>
public class TargetOverlayController : MonoBehaviour {

    private const float pixelDelta = 20;
    //private const float voxelDelta = .3f;

    [SerializeField]
    private Text templateMass = null;
    [SerializeField]
    private Text templateActualForce = null;

    private Text[] pullTargetsSumForce;
    private Text[] pushTargetsSumForce;
    private Text[] pullTargetsActualForce;
    private Text[] pushTargetsActualForce;
    //private Text highlightedTargetMass;

    void Awake() {
        pullTargetsSumForce = new Text[TargetArray.largeArrayCapacity];
        pushTargetsSumForce = new Text[TargetArray.largeArrayCapacity];
        pullTargetsActualForce = new Text[TargetArray.largeArrayCapacity];
        pushTargetsActualForce = new Text[TargetArray.largeArrayCapacity];

        //highlightedTargetMass = Instantiate(templateMass, transform, false);
        //highlightedTargetMass.text = "";

        for (int i = 0; i < TargetArray.largeArrayCapacity; i++) {
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


    public void Clear() {
        HardRefresh();
        //highlightedTargetMass.text = "";
        SetPullTextColorWeak();
        SetPushTextColorWeak();
    }

    #region overlayUpdating
    /// <summary>
    /// Called when the number of marked targets changes
    /// </summary>
    public void HardRefresh() {
        SoftRefresh();
        for (int i = Player.PlayerIronSteel.PullTargets.Count; i < TargetArray.largeArrayCapacity; i++) {
            pullTargetsSumForce[i].text = "";
            pullTargetsActualForce[i].text = "";
        }

        for (int i = Player.PlayerIronSteel.PushTargets.Count; i < TargetArray.largeArrayCapacity; i++) {
            pushTargetsSumForce[i].text = "";
            pushTargetsActualForce[i].text = "";
        }
    }

    /// <summary>
    /// Called when the position of targets or strength of forces changes
    /// </summary>
    public void SoftRefresh() {
        if (HUD.IsOpen) {
            if (SettingsMenu.settingsData.hudForces == 1) {
                SoftRefreshTargets(Player.PlayerIronSteel.PullTargets, pullTargetsActualForce, pullTargetsSumForce, SettingsMenu.settingsData.forceComplexity == 1);
                SoftRefreshTargets(Player.PlayerIronSteel.PushTargets, pushTargetsActualForce, pushTargetsSumForce, SettingsMenu.settingsData.forceComplexity == 1);
            }
            // If the target is highlighted and on screen, display mass
            if (SettingsMenu.settingsData.hudMasses == 1) {
                //if (Player.PlayerIronSteel.HasHighlightedTarget) {
                //    //Vector3 heightToTop = Vector3.zero;
                //    //heightToTop.y = Player.PlayerIronSteel.HighlightedTarget.ColliderBodyBoundsSizeY / 2f;

                //    //highlightedTargetMass.text = HUD.MassString(Player.PlayerIronSteel.HighlightedTarget.MagneticMass);
                //    //highlightedTargetMass.transform.position = CameraController.ActiveCamera.WorldToScreenPoint(Player.PlayerIronSteel.HighlightedTarget.transform.position + heightToTop) + new Vector3(0, pixelDelta);
                //} else { // Target is not highlighted or is not on screen, hide mass label
                //    highlightedTargetMass.text = "";
                //}
            }
        }
    }

    /// <summary>
    /// Called when settings change, and the fundamental overlay might also have to change
    /// </summary>
    public void InterfaceRefresh() {
        if (HUD.IsOpen) {
            //if (SettingsMenu.settingsData.hudMasses == 0) {
            //    highlightedTargetMass.text = "";
            //}
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
    }

    /// <summary>
    /// Refreshes the labels for over the marked targets.
    /// </summary>
    /// <param name="targets">the array of marked targets</param>
    /// <param name="actualForce">the array of Allomantic Forces acting on the targets</param>
    /// <param name="sumForce">the array of net forces acting on them</param>
    /// <param name="refreshSum">true if the net force needs to be updated (it might be invisible)</param>
    private void SoftRefreshTargets(TargetArray targets, Text[] actualForce, Text[] sumForce, bool refreshSum) {
        for (int i = 0; i < targets.Count; i++) {
            Magnetic target = targets[i];

            Vector3 heightToTop = Vector3.zero;
            heightToTop.y = target.ColliderBodyBoundsSizeY / 2f;

            Vector3 positionActualForce = CameraController.ActiveCamera.WorldToScreenPoint(target.transform.position - heightToTop) + new Vector3(0, -pixelDelta);

            if (positionActualForce.z > 0) {
                actualForce[i].transform.position = positionActualForce;
                actualForce[i].text = HUD.ForceString(target.LastNetForceOnTarget.magnitude, target.NetMass);
                if (refreshSum)
                    sumForce[i].text = HUD.AllomanticSumString(target.LastAllomanticForce, target.LastAnchoredPushBoostFromAllomancer, target.NetMass, 2, true);
            } else { // Target is not on screen
                sumForce[i].text = "";
                actualForce[i].text = "";
            }
        }
    }
    #endregion

    public void SetPullTextColorStrong() {
        for (int i = 0; i < TargetArray.largeArrayCapacity; i++) {
            pullTargetsActualForce[i].color = HUD.strongBlue;
        }
    }

    public void SetPushTextColorStrong() {
        for (int i = 0; i < TargetArray.largeArrayCapacity; i++) {
            pushTargetsActualForce[i].color = HUD.strongBlue;
        }
    }

    public void SetPullTextColorWeak() {
        for (int i = 0; i < TargetArray.largeArrayCapacity; i++) {
            pullTargetsActualForce[i].color = HUD.weakBlue;
        }
    }

    public void SetPushTextColorWeak() {
        for (int i = 0; i < TargetArray.largeArrayCapacity; i++) {
            pushTargetsActualForce[i].color = HUD.weakBlue;
        }
    }

    public void RemoveHighlightedTargetLabel() {
        //highlightedTargetMass.text = "";
    }
}
