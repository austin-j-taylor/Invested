using UnityEngine;
using System.Collections;

/// <summary>
/// Marks an object that a Waddler can pick up.
/// </summary>
public class Crate : Magnetic {

    // When a Waddler starts moving towards this crate to pick it up, this is true.
    // It's used to prevent multiple Waddlers from going to the same crate.
    public bool IsATarget = false;
    //public bool IsATarget { get; set; } = false;

    private void OnDestroy() {
        GameManager.Props.RemoveProp(this);
    }
    public override void OnDisable() {
        base.OnDisable();
        GameManager.Props.RemoveProp(this);
    }
    public override void OnEnable() {
        base.OnEnable();
        GameManager.Props.AddProp(this);
    }

}
