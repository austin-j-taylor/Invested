using UnityEngine;
using System.Collections;
/// <summary>
/// Makes the player's body appear transparent when the camera is looking at it/near it
/// or in first person mode.
/// </summary>
public class ActorTransparencyController : MonoBehaviour {

    protected Renderer[] rends = null; // assigned by child
    protected bool isOpaque, overrideHidden = false, isHidden = false;
    
    public void SetOverrideHidden(bool hidden) {
        if (hidden != overrideHidden) {
            overrideHidden = hidden;
            isHidden = hidden;
            if(rends != null)
                foreach (Renderer rend in rends) {
                    if (rend) {
                        rend.enabled = !hidden;
                    }
                }
        }
    }
}
