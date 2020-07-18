using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KogActor : MonoBehaviour {

    // State machine for Kog
    public enum State { Resting, Reaching, Throwing, Meditating };

    private KogAnimation kogAnimation;

    private void Awake() {
        kogAnimation = GetComponentInChildren<KogAnimation>();
    }

}
