﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour {

    const int threadGroupSize = 1024;

    [SerializeField]
    private Transform target = null;

    public BoidSettings settings;
    public ComputeShader compute;
    Boid[] boids;
    void Start() {
        target = Player.PlayerInstance.transform;

        boids = FindObjectsOfType<Boid>();
        foreach (Boid b in boids) {
            b.Initialize(settings, target);
        }
    }

    void FixedUpdate() {
        if (boids != null) {

            int numBoids = boids.Length;
            BoidData[] boidData = new BoidData[numBoids];

            for (int i = 0; i < boids.Length; i++) {
                boidData[i].position = boids[i].transform.position;
                boidData[i].direction = boids[i].transform.forward;
            }

            ComputeBuffer boidBuffer = new ComputeBuffer(numBoids, BoidData.Size);
            boidBuffer.SetData(boidData);

            compute.SetBuffer(0, "boids", boidBuffer);
            compute.SetInt("numBoids", boids.Length);
            compute.SetFloat("viewRadius", settings.perceptionRadius);
            compute.SetFloat("avoidRadius", settings.avoidanceRadius);

            int threadGroups = Mathf.CeilToInt(numBoids / (float)threadGroupSize);
            compute.Dispatch(0, threadGroups, 1, 1);

            boidBuffer.GetData(boidData);

            for (int i = 0; i < boids.Length; i++) {
                boids[i].FixedUpdateBoid(boidData[i].flockHeading, boidData[i].flockCentre, boidData[i].avoidanceHeading, boidData[i].numFlockmates);
            }

            boidBuffer.Release();
        }
    }

    public struct BoidData {
        public Vector3 position;
        public Vector3 direction;

        public Vector3 flockHeading;
        public Vector3 flockCentre;
        public Vector3 avoidanceHeading;
        public int numFlockmates;

        public static int Size {
            get {
                return sizeof(float) * 3 * 5 + sizeof(int);
            }
        }
    }
}