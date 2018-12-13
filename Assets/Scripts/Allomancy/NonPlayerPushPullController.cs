using UnityEngine;
using VolumetricLines;

/*
 * The AllomanticIronSteel used by non-player Allomancers.
 * Controls the blue metal lines that point from the the Allomancer to selected targets.
 */
public class NonPlayerPushPullController : AllomanticIronSteel {
    
    public bool LinesAreVisibleWhenNotBurning {
        set {
            if(value) {
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

    public VolumetricLineBehavior[] pullLines;
    public VolumetricLineBehavior[] pushLines;

    protected override void Awake() {
        base.Awake();
        IronReserve.IsEndless = true;
        SteelReserve.IsEndless = true;
        
        pullLines = new VolumetricLineBehavior[maxNumberOfTargets];
        pushLines = new VolumetricLineBehavior[maxNumberOfTargets];
        for (int i = 0; i < maxNumberOfTargets; i++) {
            pullLines[i] = Instantiate(GameManager.MetalLineTemplate);
            pushLines[i] = Instantiate(GameManager.MetalLineTemplate);
        }
    }

    private void Update() {
        if (!PauseMenu.IsPaused) {
            for (int i = 0; i < maxNumberOfTargets; i++) {
                // Non-existent target, disable blue line
                if (PullTargets[i] == null) {
                    pullLines[i].GetComponent<MeshRenderer>().enabled = false;
                } else {
                    UpdateLines(PullTargets, pullLines, i);
                }
                if (PushTargets[i] == null) {
                    pushLines[i].GetComponent<MeshRenderer>().enabled = false;
                } else {
                    UpdateLines(PushTargets, pushLines, i);
                }
            }

        }
    }

    private void UpdateLines(TargetArray array, VolumetricLineBehavior[] lines, int index) {
        float allomanticForce = CalculateAllomanticForce(array[index], this).magnitude;

        //allomanticForce -= SettingsMenu.settingsData.metalDetectionThreshold; // blue metal lines will fade to a luminocity of 0 when the force is on the edge of the threshold

        float closeness = Mathf.Exp(-blueLineStartupFactor * Mathf.Pow(1 / allomanticForce, blueLineBrightnessFactor));

        lines[index].GetComponent<MeshRenderer>().enabled = true;
        lines[index].StartPos = array[index].CenterOfMass;
        lines[index].EndPos = CenterOfMass;
        lines[index].LineWidth = blueLineWidthBaseFactor * array[index].Charge;
        lines[index].LightSaberFactor = 1;
        lines[index].LineColor = new Color(0, closeness * lowLineColor, closeness * highLineColor, 1);
    }
}
