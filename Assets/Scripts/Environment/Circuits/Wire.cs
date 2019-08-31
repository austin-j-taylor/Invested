using UnityEngine;
using System.Collections;

public class Wire : Powered {
    
    public override bool On {
        set {
            base.On = value;
            rend.material = value ? GameManager.Material_MARLmetal_lit : GameManager.Material_MARLmetal_unlit;
        }
    }

    private Renderer rend;

    private void Awake() {
        rend = GetComponentInChildren<Renderer>();
    }
}
