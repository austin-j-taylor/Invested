using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Controls the HUD element that overlays a hostile entity.
/// Includes the reticle element that appears over a target when aiming at it.
/// </summary>
public class TargetEntityController : MonoBehaviour {

    private const int offset = 42;

    private enum ControlState { None, Aiming };

    private Animator anim;
    private ControlState state;

    private Image reticle;
    private Bounds lastMarkedBounds;

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
                lastMarkedBounds = Kog.HandController.MarkedEntity.BoundingBox.bounds;
                break;
        }
        if (reticle.color.a > 0.001f) {

            Vector3 v_dx = CameraController.ActiveCamera.transform.right * lastMarkedBounds.size.x / 2f;
            Vector3 v_dy = CameraController.ActiveCamera.transform.up * lastMarkedBounds.size.y / 2f;
            Vector3 screen_center = CameraController.ActiveCamera.WorldToScreenPoint(lastMarkedBounds.center);
            Vector3 screen_dx = CameraController.ActiveCamera.WorldToScreenPoint(lastMarkedBounds.center + v_dx) - screen_center;
            Vector3 screen_dy = CameraController.ActiveCamera.WorldToScreenPoint(lastMarkedBounds.center + v_dy) - screen_center;
            reticle.rectTransform.offsetMin = new Vector2(-screen_dx.x - offset, -screen_dy.y - offset);
            reticle.rectTransform.offsetMax = new Vector2(screen_dx.x + offset, screen_dy.y + offset);
            reticle.rectTransform.position = screen_center;
            Debug.Log("transforming");
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
