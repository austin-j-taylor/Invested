using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargStationaryController : Targ {

    protected override void Start() {
        base.Start();

        MaxHealth = 10;
    }
}
