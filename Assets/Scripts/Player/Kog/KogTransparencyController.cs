using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KogTransparencyController : ActorTransparencyController {

    [SerializeField]
    private Renderer[] headRends = null;

    private void Awake() {
        rends = headRends;
    }
}
