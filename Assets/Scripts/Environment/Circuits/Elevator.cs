using UnityEngine;
using UnityEngine.Animations;
using System.Collections;

/*
 * An elevator that travels up and down a shaft as the Sphere controls it.
 * When On, the elevator reduces its weight with fIron.
 */
public class Elevator : Interfaceable {

    protected new readonly Vector2 cameraDistance = new Vector2(12, 5);
    private const float upwardsSpeed = 5;
    private const float downwardsSpeed = -1;

    [SerializeField]
    private Magnetic floorAnchor = null, ceilingAnchor = null;
    private Magnetic thisMagnetic;

    private Animator anim;
    private Rigidbody rb;
    //private ParentConstraint pc;
    private Vector3 offset;

    private void Awake() {
        anim = GetComponentInChildren<Animator>();
        thisMagnetic = GetComponentInChildren<Magnetic>();
        rb = GetComponent<Rigidbody>();
        //pc = GetComponent<ParentConstraint>();
        //ConstraintSource source = new ConstraintSource {
        //    weight = 1,
        //    sourceTransform = Player.PlayerInstance.transform
        //};
        //pc.SetSource(0, source);
    }

    protected override void StartInterfacing() {
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        Player.PlayerIronSteel.rb.velocity = Vector3.zero;
        Player.PlayerIronSteel.Clear();
        Player.PlayerIronSteel.Strength = 1000;
        Player.PlayerIronSteel.StartBurning();
        Player.PlayerIronSteel.AddPushTarget(floorAnchor);
        Player.PlayerIronSteel.AddPullTarget(thisMagnetic);
        Player.PlayerIronSteel.AddPullTarget(ceilingAnchor);
        //Player.PlayerIronSteel.AddPullTarget(thisMagnetic);
        CameraController.ExternalDistance = cameraDistance;
        Player.PlayerIronSteel.ExternalControl = true;
        Player.PlayerIronSteel.rb.angularVelocity = Vector3.zero;
        offset = transform.position - Player.PlayerInstance.transform.position;
        anim.SetTrigger("flickerOn");

        //Vector3 positionOffset = Player.PlayerInstance.transform.InverseTransformPoint(transform.position);
        //Quaternion rotationOffset = Quaternion.Inverse(Player.PlayerInstance.transform.rotation) * transform.rotation;
        //pc.SetTranslationOffset(0, positionOffset);
        //pc.SetRotationOffset(0, rotationOffset.eulerAngles);

        //pc.constraintActive = true;
        //rb.isKinematic = true;
        Player.PlayerIronSteel.rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePosition & ~RigidbodyConstraints.FreezePositionY;
        //thisMagnetic.gameObject.SetActive(false);
    }
    protected override void StopInterfacing() {
        rb.isKinematic = true;
        Player.CanControl = true;
        Player.PlayerIronSteel.StopBurning();
        Player.PlayerIronSteel.Strength = 1;
        CameraController.ExternalDistance = Vector2.zero;
        Player.PlayerIronSteel.ExternalControl = false;
        anim.SetTrigger("turnOff");
        //pc.constraintActive = false;
        //rb.isKinematic = false;
        Player.PlayerIronSteel.rb.constraints = RigidbodyConstraints.None;
        //thisMagnetic.gameObject.SetActive(true);
    }
    protected override void FixedUpdateInterfacing() {
        Player.PlayerIronSteel.IronPulling = true;
        Player.PlayerIronSteel.SteelPushing = true;

        if (Keybinds.SteelPushing()) { // go up
            Player.PlayerIronSteel.ExternalCommand = 2 * -Physics.gravity.y * ((Player.PlayerIronSteel.Mass));
        } else if (Keybinds.IronPulling()) { // go down
            Player.PlayerIronSteel.ExternalCommand = 0;// -Physics.gravity.y * ((Player.PlayerIronSteel.Mass + rb.mass));
        } else { // balance
            //float speed = rb.velocity.y;
            //float direction = Mathf.Sign(speed);
            //speed = Mathf.Abs(speed);
            //speed = 1 - Mathf.Exp(-speed / 5);
            //float factor = 1 - direction * speed;
            //Player.PlayerIronSteel.ExternalCommand = factor * -Physics.gravity.y * ((Player.PlayerIronSteel.Mass + rb.mass));
            //Debug.Log("raw:" + rb.velocity);
            //Debug.Log("Speed:" + speed);
            //Debug.Log("facto:" + factor);
            Player.PlayerIronSteel.ExternalCommand = 1 * -Physics.gravity.y * ((Player.PlayerIronSteel.Mass));

        }

        Debug.Log("Wanted: " + Player.PlayerIronSteel.ExternalCommand);
        Debug.Log("Have  : " + Player.PlayerIronSteel.LastMaximumNetForce.magnitude);
        Debug.Log("Net force: " + Player.PlayerIronSteel.LastNetForceOnAllomancer);
    }
    protected override void UpdateInterfacing() {
        if (Keybinds.Jump())
            Interfaced = false;
    }

    private void LateUpdate() {
        if(Interfaced) {
            Vector3 temp = transform.position;
            temp.y = Player.PlayerInstance.transform.position.y + offset.y;
            transform.position = temp;
            rb.velocity = Player.PlayerIronSteel.rb.velocity;
        }
    }

    protected override IEnumerator Interaction() {
        //ReceivedReply = false;

        //HUD.ConsoleController.Log(" > ");
        //yield return new WaitForSeconds(2);
        //HUD.ConsoleController.TypeInLine("Please give me access to this elevator, kind soul.", this);
        //while(!ReceivedReply) {
        yield return null;
        //}
        //ReceivedReply = false;
        //yield return new WaitForSeconds(.1f);
        //HUD.ConsoleController.LogLine("_CONTROLLER_INIT()");
        //yield return new WaitForSeconds(.7f);
        //HUD.ConsoleController.LogLine("_SSSH_VALIDATE()");
        //yield return new WaitForSeconds(.3f);
        //HUD.ConsoleController.LogLine("[CRITICAL] Your SSSH certificate is out of date by 212 months. Please contact your spiritual service provider.");
        //yield return new WaitForSeconds(1.1f);
        //HUD.ConsoleController.LogLine("_AI_INIT()");
        //yield return new WaitForSeconds(1f);
        //HUD.ConsoleController.LogLine("_F_IRON_INIT()");
        //yield return new WaitForSeconds(.75f);
        //HUD.ConsoleController.LogLine("_START_COROUTINE_F_IRON_STORE()");
        //yield return new WaitForSeconds(.5f);
        //HUD.ConsoleController.LogLine("_DOORS_INIT()");

        //HUD.ConsoleController.Log(" > ");
        //yield return new WaitForSeconds(1f);
        //HUD.ConsoleController.TypeInLine("Please unlock the doors to the upper facilities, kind soul.", this);
        //while (!ReceivedReply) {
        //    yield return null;
        //}
        //ReceivedReply = false;

        Interfaced = true;

        //yield return new WaitForSeconds(.25f);
        //HUD.ConsoleController.LogLine("Unlocking doors to upper facilities ...");
        //yield return new WaitForSeconds(2);
        //HUD.ConsoleController.LogLine("Door 0 unlocked.");
        //yield return new WaitForSeconds(.1f);
        //HUD.ConsoleController.LogLine("[ERROR] No response received from door 1.");
        //yield return new WaitForSeconds(.025f);
        //HUD.ConsoleController.LogLine("[ERROR] No response received from door 2.");
        //yield return new WaitForSeconds(.025f);
        //HUD.ConsoleController.LogLine("[ERROR] No response received from door 3.");
        //yield return new WaitForSeconds(.025f);
        //HUD.ConsoleController.LogLine("[ERROR] No response received from door 4.");
        //yield return new WaitForSeconds(2);

        //HUD.ConsoleController.Log(" > ");
        //yield return new WaitForSeconds(1);
        //HUD.ConsoleController.TypeInLine("Thank you for your time, kind soul. I will come for you will I learn what happened here.", this);
        //while (!ReceivedReply) {
        //    yield return null;
        //}
        //ReceivedReply = false;
        
        //yield return new WaitForSeconds(5);
        //HUD.MessageOverlayCinematic.FadeIn("Press " + TextCodes.Space + " to exit the console.");
        //while(HUD.ConsoleController.IsOpen) {
        //    yield return null;
        //}
        //if(Keybinds.Jump()) {
        //    HUD.MessageOverlayCinematic.FadeOut();
        //    HUD.ConsoleController.Close();
        //    Player.CanControl = true;
        //}
    }

    private float getPercentage(float speed) {
        Debug.Log("Wanted: " + ((rb.mass + Player.PlayerIronSteel.Mass) * (1 + speed) * -Physics.gravity.y));
        Debug.Log("Have  : " + Player.PlayerIronSteel.LastMaximumNetForce.magnitude);
        Debug.Log("Net force: " + Player.PlayerIronSteel.LastAllomanticForce);
        return ((rb.mass + Player.PlayerIronSteel.Mass) * (1 + speed) * -Physics.gravity.y) / Player.PlayerIronSteel.LastMaximumNetForce.magnitude;
    }
}
