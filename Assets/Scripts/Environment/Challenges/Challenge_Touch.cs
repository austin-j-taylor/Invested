using UnityEngine;
using System.Collections;

// Just touching the trigger completes the challenge.
public class Challenge_Touch : Challenge {

    protected override void IntroduceChallenge() {
        base.IntroduceChallenge();
        CompleteChallenge();
    }
}
