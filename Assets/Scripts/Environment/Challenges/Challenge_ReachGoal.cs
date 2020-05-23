using UnityEngine;
using System.Collections;

// Challenge to simply reach the goal
public class Challenge_ReachGoal : Challenge_TimeTrial {


    protected override void Start() {
        base.Start();
        challengeType = "Reach the Goal: ";
        if (failureObjects.Length > 0)
            challengeDescription = challengeDescription + TextCodes.Red("\n - The Floor is Lava: Touching the ground fails the challenge.");
    }
}
