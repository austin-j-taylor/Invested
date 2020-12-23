using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemFollowingPlayer : MonoBehaviour {

    [SerializeField]
    private bool followVertically = true;

    private float startY;

    private void Start() {
        startY = transform.position.y;
        Update();
        ParticleSystem particles = GetComponent<ParticleSystem>();
        particles.Stop();
        particles.Play();
    }

    private void Update() {
        Vector3 position = Player.PlayerInstance.transform.position;

        if (!followVertically) {
            position.y = startY;
        }
        transform.position = position;
    }
}
