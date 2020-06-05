using UnityEngine;
using System.Collections;

// Challenge to simply reach the goal
public class Challenge_ReachGoal : Challenge_TimeTrial {


    protected override void Start() {
        base.Start();
        challengeType = "Reach the Goal: ";
    }
}
