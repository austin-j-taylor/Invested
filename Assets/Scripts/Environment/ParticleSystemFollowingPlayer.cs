using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemFollowingPlayer : MonoBehaviour {

    [SerializeField]
    private bool followVertically = true;
    private Transform followTarget;

    private ParticleSystem particles;
    private float startY;

    private void Awake() {
        followTarget = Player.PlayerInstance.transform;
    }

    private void Start() {
        startY = transform.position.y;
        Update();
        particles = GetComponent<ParticleSystem>();
        particles.Stop();
        particles.Play();
    }

    private void Update() {
        Vector3 position = followTarget.position;

        if (!followVertically) {
            position.y = startY;
        }
        transform.position = position;
    }

    public void SetFollowTarget(Transform newTarget) {
        followTarget = newTarget;
        transform.position = followTarget.position;
        particles.Clear();
        particles.Simulate(particles.main.duration);
        particles.Play();
    }
}
