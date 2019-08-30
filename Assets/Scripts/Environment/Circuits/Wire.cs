using UnityEngine;
using System.Collections;

public class Wire : Powered {
    
    public override bool On {
        set {
            base.On = value;
            rend.material = value ? GameManager.Material_MARL_Wire_lit : GameManager.Material_MARL_Wire_unlit;
        }
    }

    private Renderer rend;

    private void Awake() {
        rend = GetComponentInChildren<Renderer>();
    }
}
