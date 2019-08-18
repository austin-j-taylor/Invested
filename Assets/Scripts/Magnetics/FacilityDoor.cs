using UnityEngine;
using System.Collections;

public class FacilityDoor : MonoBehaviour {

    private const float angleForStopping = 1;
    private const int timeToTryHarder = 2;
    private const int timeToQuit = 4;

    private bool locked;
    public bool Locked {
        get {
            return locked;
        }
        set {
            if (locked != value) {
                locked = value;
                if(locked) {
                    rendererLeft.material = lockedMaterial;
                    rendererRight.material = lockedMaterial;
                    magneticLeft.enabled = false;
                    magneticRight.enabled = false;
                    StartCoroutine(SpringToLock());
                } else {
                    StopAllCoroutines();
                    jointLeft.spring = lowSpring;
                    jointRight.spring = lowSpring;
                    jointLeft.useLimits = false;
                    jointRight.useLimits = false;
                    rendererLeft.material = unlockedMaterial;
                    rendererRight.material = unlockedMaterial;
                    magneticLeft.enabled = true;
                    magneticRight.enabled = true;
                }
            }
        }
    }

    private HingeJoint jointLeft;
    private HingeJoint jointRight;
    private JointSpring lowSpring;
    private JointSpring highSpring;

    private Renderer rendererLeft;
    private Renderer rendererRight;
    private Material lockedMaterial;
    [SerializeField]
    private Material unlockedMaterial = null;

    private Magnetic magneticLeft;
    private Magnetic magneticRight;

    private void Start() {
        jointLeft = transform.Find("Left").GetComponent<HingeJoint>();
        jointRight = transform.Find("Right").GetComponent<HingeJoint>();

        lowSpring = jointLeft.spring;
        highSpring = jointLeft.spring;
        lowSpring.spring = 25;
        highSpring.spring = 300;
        jointLeft.spring = lowSpring;
        jointRight.spring = lowSpring;

        Transform leftMetal = jointLeft.transform.Find("Left_metal");
        Transform rightMetal = jointRight.transform.Find("Right_metal");

        rendererLeft = leftMetal.GetComponent<Renderer>();
        rendererRight = rightMetal.GetComponent<Renderer>();
        lockedMaterial = rendererLeft.material;

        magneticLeft = leftMetal.GetComponent<Magnetic>();
        magneticRight = rightMetal.GetComponent<Magnetic>();

        Locked = true;
    }

    private void Update() {
        Locked = Keybinds.ZincTimeDown() ? !Locked : locked;
        //if(!locked) {
        //    // when the door is nearly closed, try harder to seal it shut
        //    if(((jointLeft.angle < 0) ? -jointLeft.angle : jointLeft.angle) > angleForStopping
        //             || ((jointRight.angle < 0) ? -jointRight.angle : jointRight.angle) > angleForStopping)) {
        //        jointLeft.spring = highSpring;
        //        jointRight.spring = highSpring;
        //    }
        //}

    }

    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("PlayerBody")) {
            Locked = !locked;
        }
    }

    private IEnumerator SpringToLock() {
        jointLeft.spring = highSpring;
        jointRight.spring = highSpring;
        float timer = 0;
        while (timer < timeToQuit && 
                (
                        ((jointLeft.angle < 0) ? -jointLeft.angle : jointLeft.angle) > angleForStopping
                     || ((jointRight.angle < 0) ? -jointRight.angle : jointRight.angle) > angleForStopping)
                ) {

            timer += Time.deltaTime;
            if (timer > timeToTryHarder) { // player is being annoying and trying to block the doorway
                JointSpring reallyHighSpring = jointLeft.spring;
                reallyHighSpring.spring = 10000;
                jointLeft.spring = reallyHighSpring;
                jointRight.spring = reallyHighSpring;
            }

            yield return null;
        }
        jointLeft.useLimits = true;
        jointRight.useLimits = true;

    }
}
