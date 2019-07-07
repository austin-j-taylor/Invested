using UnityEngine;
using VolumetricLines;

/*
 * The AllomanticIronSteel used by non-player Allomancers.
 * Controls the blue metal lines that point from the this non-player Allomancer to the Player, if this non-player Allomancer is pushing/pulling on the Player.
 */
public class NonPlayerPushPullController : AllomanticIronSteel {
    
    public VolumetricLineBehavior[] pullLines;
    public VolumetricLineBehavior[] pushLines;

    public bool LinesAreVisible {
        set {
            if (value) {
                for (int i = 0; i < maxNumberOfTargets; i++) {
                    pullLines[i].gameObject.layer = GameManager.Layer_BlueLinesVisible;
                    pushLines[i].gameObject.layer = GameManager.Layer_BlueLinesVisible;
                }
            } else {
                for (int i = 0; i < maxNumberOfTargets; i++) {
                    pullLines[i].gameObject.layer = GameManager.Layer_BlueLines;
                    pushLines[i].gameObject.layer = GameManager.Layer_BlueLines;
                }
            }
        }
    }

    protected override void Awake() {
        base.Awake();
        IronReserve.IsEndless = true;
        SteelReserve.IsEndless = true;
        PullTargets.MaxRange = -1;
        PushTargets.MaxRange = -1;

        pullLines = new VolumetricLineBehavior[maxNumberOfTargets];
        pushLines = new VolumetricLineBehavior[maxNumberOfTargets];
        for (int i = 0; i < maxNumberOfTargets; i++) {
            pullLines[i] = Instantiate(GameManager.MetalLineTemplate);
            pushLines[i] = Instantiate(GameManager.MetalLineTemplate);
        }

        LinesAreVisible = true;
    }

    private void LateUpdate() {
        if (!PauseMenu.IsPaused) {
            for (int i = 0; i < maxNumberOfTargets; i++) {
                // Non-existent target, disable blue line
                //if (PullTargets[i] != Player.PlayerMagnetic || !IronPulling) {
                if (i >= PullTargets.Count || !IronPulling) {
                    pullLines[i].gameObject.SetActive(false);
                } else {
                    UpdateLines(true, i);
                }
                //if (PushTargets[i] != Player.PlayerMagnetic || !SteelPushing) {
                if (i >= PushTargets.Count || !SteelPushing) {
                    pushLines[i].gameObject.SetActive(false);
                } else {
                    UpdateLines(false, i);
                }
            }

        }
    }

    private void UpdateLines(bool pulling, int index) {
        
        Magnetic target;
        float percentage;
        VolumetricLineBehavior line;
        if (pulling) {
            target = PullTargets[index];
            line = pullLines[index];
            percentage = IronBurnPercentageTarget;
        } else {
            target = PushTargets[index];
            line = pushLines[index];
            percentage = SteelBurnPercentageTarget;
        }

        float allomanticForce = CalculateAllomanticForce(target).magnitude;

        //allomanticForce -= (allomancer.MaxRange == 0 ? SettingsMenu.settingsData.metalDetectionThreshold : allomancer.MaxRange); // blue metal lines will fade to a luminocity of 0 when the force is on the edge of the threshold

        float closeness = Mathf.Exp(-blueLineChangeFactor * Mathf.Pow(1 / allomanticForce, blueLineBrightnessFactor));

        line.gameObject.SetActive(true);
        line.StartPos = target.CenterOfMass;
        line.EndPos = CenterOfMass;
        line.LineWidth = target.Charge * (SettingsMenu.settingsData.cameraFirstPerson == 0 ? blueLineThirdPersonWidth : blueLineFirstPersonWidth);
        line.LightSaberFactor = Mathf.Exp(-target.LastMaxPossibleAllomanticForce.magnitude * percentage / TargetArray.lightSaberConstant);
        line.LineColor = pulling ? new Color(0, closeness * lowLineColor, closeness * highLineColor, 1) : TargetArray.targetedRedLine * closeness;
    }
}
