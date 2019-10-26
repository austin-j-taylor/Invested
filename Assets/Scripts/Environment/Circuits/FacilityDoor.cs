using UnityEngine;
using System.Collections;

public class FacilityDoor : Powered {

    private const float angleForStopping = 1;
    private const int timeToTryHarder = 2;
    private const int timeToQuit = 4;

    [SerializeField]
    private bool lockOncePassed = true;
    
    public override bool On {
        set {
            if (On != value) {
                if(value) {
                    rendererLeft.material = GameManager.Material_MARLmetal_lit;
                    rendererRight.material = GameManager.Material_MARLmetal_lit;
                    magneticLeft.enabled = false;
                    magneticRight.enabled = false;
                    StartCoroutine(SpringToLock());
                } else {
                    StopAllCoroutines();
                    jointLeft.spring = lowSpring;
                    jointRight.spring = lowSpring;
                    jointLeft.useLimits = false;
                    jointRight.useLimits = false;
                    rendererLeft.material = GameManager.Material_MARLmetal_unlit;
                    rendererRight.material = GameManager.Material_MARLmetal_unlit;
                    magneticLeft.enabled = true;
                    magneticRight.enabled = true;
                }
            }
            base.On = value;
        }
    }

    private HingeJoint jointLeft;
    private HingeJoint jointRight;
    private JointSpring lowSpring;
    private JointSpring highSpring;

    private Renderer rendererLeft;
    private Renderer rendererRight;

    private Magnetic magneticLeft;
    private Magnetic magneticRight;

    private void Awake() {
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

        magneticLeft = leftMetal.GetComponent<Magnetic>();
        magneticRight = rightMetal.GetComponent<Magnetic>();

        On = true;
        magneticLeft.enabled = false;
        magneticRight.enabled = false;
        jointLeft.useLimits = true;
        jointRight.useLimits = true;
    }

    //private void Update() {
    //    if (!On) {
    //        // when the door is nearly closed, try harder to seal it shut
    //        if (((jointLeft.angle < 0) ? -jointLeft.angle : jointLeft.angle) > angleForStopping
    //                 || ((jointRight.angle < 0) ? -jointRight.angle : jointRight.angle) > angleForStopping)) {
    //            jointLeft.spring = highSpring;
    //            jointRight.spring = highSpring;
    //        }
    //    }

    //}

    // close the door when the player passes it
    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Player") && !other.isTrigger) {
            if(lockOncePassed) {
                On = true;
            } else {
                On = !On;
            }
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
