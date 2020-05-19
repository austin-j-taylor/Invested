using UnityEngine;
using System.Collections;

public class Challenge_Touch : Challenge {

    private Collider sphere;

    private void Start() {
        sphere = GetComponentInChildren<Collider>();
        sphere.gameObject.AddComponent<ChallengeTrigger>().parent = this;
        
    }
    protected override void StartChallenge() {
        base.StartChallenge();
        sphere.gameObject.SetActive(false);
        CompleteChallenge();
    }
}
