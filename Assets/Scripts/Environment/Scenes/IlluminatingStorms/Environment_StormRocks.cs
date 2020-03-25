using UnityEngine;
using System.Collections;

public class Environment_StormRocks : MonoBehaviour {


    [SerializeField]
    private GameObject[] prefabBoulders = null;
    [SerializeField]
    private int spawnCount = 50;
    [SerializeField]
    private int startSpeed = 25;
    [SerializeField]
    private int startSpeedAngular = 1;

    Rigidbody[] rbs;
    // Use this for initialization
    void Start() {
        // wrong scene was started
        if(!GameManager.MetalLineTemplate) {
            return;
        }

        //randomly spawn boulders
        Transform spawn = transform.Find("Spawn");
        for(int i = 0; i < spawnCount; i++) {
            Transform boulder = Instantiate(prefabBoulders[Random.Range(0, prefabBoulders.Length - 1)], transform).transform;
            Vector3 pos = new Vector3 {
                x = spawn.position.x + spawn.localScale.x * (Random.value - .5f),
                y = spawn.position.y + spawn.localScale.y * (Random.value - .5f),
                z = spawn.position.z + spawn.localScale.z * (Random.value - .5f)
            };
            boulder.position = pos;
            Rigidbody rb = boulder.GetComponent<Rigidbody>();
            rb.velocity = Vector3.left * (startSpeed + Random.value * startSpeed);
            rb.angularVelocity = Random.insideUnitSphere * startSpeedAngular / 2 + (Vector3.forward) * startSpeedAngular;

            boulder.forward = Random.insideUnitSphere ;
        }

        rbs = GetComponentsInChildren<Rigidbody>();

        // randomly set velocities, spins
        rbs[0].velocity = Vector3.forward * 100;
        rbs[1].velocity = Vector3.forward * 70 + Vector3.down * 70;

    }

}
