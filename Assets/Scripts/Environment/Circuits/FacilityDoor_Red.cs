using UnityEngine;
using System.Collections;

public class FacilityDoor_Red : Powered {

    private const float angleForStopping = 1;
    private const int timeToTryHarder = 2;
    private const int timeToQuit = 4;
    private const int unlockImpulse = 200;
    private const int unlockAngle = 30;

    public override bool On {
        set {
            if (On != value) {
                if (value) {
                    Unlock();
                } else {
                    Lock();
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

    protected virtual void Awake() {
        jointLeft = transform.Find("Left").GetComponent<HingeJoint>();
        jointRight = transform.Find("Right").GetComponent<HingeJoint>();

        lowSpring = jointLeft.spring;
        highSpring = jointLeft.spring;
        lowSpring.spring = 25;
        highSpring.spring = 300;
        jointLeft.spring = lowSpring;
        jointRight.spring = lowSpring;

        rendererLeft = transform.Find("Left").GetComponent<Renderer>();
        rendererRight = transform.Find("Right").GetComponent<Renderer>();

        magneticLeft = rendererLeft.transform.Find("Left_metal").GetComponent<Magnetic>();
        magneticRight = rendererRight.transform.Find("Right_metal").GetComponent<Magnetic>();

        jointLeft.useLimits = true;
        jointRight.useLimits = true;
    }

    protected virtual void Lock() {
        rendererLeft.materials[1].CopyPropertiesFromMaterial(GameManager.Material_MARLmetal_unlit);
        rendererRight.materials[1].CopyPropertiesFromMaterial(GameManager.Material_MARLmetal_unlit);
        rendererLeft.materials[2].CopyPropertiesFromMaterial(GameManager.Material_MARLmetal_unlit);
        rendererRight.materials[2].CopyPropertiesFromMaterial(GameManager.Material_MARLmetal_unlit);
        StartCoroutine(SpringToLock());
    }
    protected virtual void Unlock() {
        StopAllCoroutines();
        GetComponent<AudioSource>().Play();
        lowSpring.targetPosition = unlockAngle;
        jointLeft.spring = lowSpring;
        lowSpring.targetPosition = -unlockAngle;
        jointRight.spring = lowSpring;
        jointLeft.useLimits = false;
        jointRight.useLimits = false;
        rendererLeft.materials[1].CopyPropertiesFromMaterial(GameManager.Material_MARLmetal_lit);
        rendererRight.materials[1].CopyPropertiesFromMaterial(GameManager.Material_MARLmetal_lit);
        rendererLeft.materials[2].CopyPropertiesFromMaterial(GameManager.Material_Steel_lit);
        rendererRight.materials[2].CopyPropertiesFromMaterial(GameManager.Material_Steel_lit);

        jointLeft.GetComponent<Rigidbody>().AddForce(transform.right * unlockImpulse, ForceMode.Impulse);
        jointRight.GetComponent<Rigidbody>().AddForce(transform.right * unlockImpulse, ForceMode.Impulse);
    }

    protected IEnumerator SpringToLock() {
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
