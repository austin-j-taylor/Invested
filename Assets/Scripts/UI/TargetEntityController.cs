using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Controls the HUD element that overlays a hostile entity.
/// Includes the reticle element that appears over a target when aiming at it.
/// </summary>
public class TargetEntityController : MonoBehaviour {

    private enum ControlState { None, Aiming };

    private Animator anim;
    private ControlState state;

    private Image reticle;

    // Use this for initialization
    void Awake() {
        anim = GetComponent<Animator>();
        state = ControlState.None;
        reticle = transform.Find("reticle").GetComponent<Image>();
    }
    public void Clear() {
        State_toNone();
    }

    private void LateUpdate() {
        // Transition
        switch (state) {
            case ControlState.None:
                if (Kog.HandController.MarkedEntity != null)
                    State_toAiming();
                break;
            case ControlState.Aiming:
                if (Kog.HandController.MarkedEntity == null)
                    State_toNone();
                break;
        }
        // Action
        switch (state) {
            case ControlState.None:
                break;
            case ControlState.Aiming:
                // Move the reticle to align with the target's transform
                Vector3 newPos = CameraController.ActiveCamera.WorldToScreenPoint(Kog.HandController.MarkedEntity.FuzzyGlobalCenterOfMass);

                reticle.rectTransform.position = newPos;
                break;
        }
    }

    private void State_toNone() {
        anim.SetBool("Aiming", false);
        state = ControlState.None;
    }
    private void State_toAiming() {
        anim.SetBool("Aiming", true);
        state = ControlState.Aiming;
    }
}
