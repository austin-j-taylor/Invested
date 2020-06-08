using UnityEngine;
using VolumetricLines;

/*
 * The AllomanticIronSteel used by non-player Allomancers.
 * Controls the blue metal lines that point from the this non-player Allomancer to the Player, if this non-player Allomancer is pushing/pulling on the Player.
 */
public class NonPlayerPushPullController : AllomanticIronSteel {

    private VolumetricLineBehavior[] pullLines;
    private VolumetricLineBehavior[] pushLines;

    public bool LinesAreVisible {
        set {
            if (value) {
                for (int i = 0; i < TargetArray.smallArrayCapacity; i++) {
                    pullLines[i].gameObject.layer = GameManager.Layer_BlueLinesVisible;
                    pushLines[i].gameObject.layer = GameManager.Layer_BlueLinesVisible;
                }
            } else {
                for (int i = 0; i < TargetArray.smallArrayCapacity; i++) {
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

        pullLines = new VolumetricLineBehavior[TargetArray.smallArrayCapacity];
        pushLines = new VolumetricLineBehavior[TargetArray.smallArrayCapacity];
        if(GameManager.MetalLineTemplate != null) {
            for (int i = 0; i < TargetArray.smallArrayCapacity; i++) {
                pullLines[i] = Instantiate(GameManager.MetalLineTemplate, GameManager.MetalLinesTransform);
                pushLines[i] = Instantiate(GameManager.MetalLineTemplate, GameManager.MetalLinesTransform);
            }
            LinesAreVisible = true;
        } else {
            enabled = false;
        }
    }
    protected override void InitArrays() {
        PullTargets = new TargetArray(TargetArray.smallArrayCapacity);
        PushTargets = new TargetArray(TargetArray.smallArrayCapacity);
    }

    private void LateUpdate() {
        if (!PauseMenu.IsPaused) {
            for (int i = 0; i < TargetArray.smallArrayCapacity; i++) {
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

        float closeness = Mathf.Exp(-PlayerPullPushController.blueLineChangeFactor * Mathf.Pow(1 / allomanticForce, PlayerPullPushController.blueLineBrightnessFactor));

        line.gameObject.SetActive(true);
        line.LightSaberFactor = Mathf.Exp(-target.LastMaxPossibleAllomanticForce.magnitude * percentage / TargetArray.lightSaberConstant);
        line.LineColor = pulling ? new Color(0, closeness * Magnetic.lowLineColor, closeness * Magnetic.highLineColor, 1) : TargetArray.targetedRedLine * closeness;
        line.SetStartAndEndAndWidth(target.CenterOfMass, CenterOfMass, target.Charge * PlayerPullPushController.blueLineWidthFactor);
    }
}
