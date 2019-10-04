using UnityEngine;
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
    private Magnetic floorAnchor = null;

    private Rigidbody rb;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    protected override void StartInterfacing() {
        Player.PlayerIronSteel.Clear();
        Player.PlayerIronSteel.Charge *= 10;
        Player.PlayerIronSteel.StartBurning();
        Player.PlayerIronSteel.AddPullTarget(GetComponentInChildren<Magnetic>());
        Player.PlayerIronSteel.AddPushTarget(floorAnchor);
        Player.PlayerIronSteel.IronPulling = true;
        CameraController.ExternalDistance = cameraDistance;
    }
    protected override void StopInterfacing() {
        Player.PlayerIronSteel.Clear();
        CameraController.ExternalDistance = Vector2.zero;
    }
    protected override void FixedUpdateInterfacing() {
        Player.PlayerIronSteel.AddPullTarget(GetComponentInChildren<Magnetic>());
        Player.PlayerIronSteel.AddPushTarget(floorAnchor);
        Player.PlayerIronSteel.IronPulling = true;
        Player.PlayerIronSteel.IronBurnPercentageTarget = .1f;
        
        Player.PlayerIronSteel.SteelPushing = true;
        if (Player.PlayerIronSteel.LastMaximumNetForce == Vector3.zero) {
            Player.PlayerIronSteel.SteelBurnPercentageTarget = .1f;
        } else {
            if(Keybinds.SteelPushing()) { // go up
                Player.PlayerIronSteel.SteelBurnPercentageTarget = getPercentage(upwardsSpeed);
            } else if(Keybinds.IronPulling()) { // go down
                Player.PlayerIronSteel.SteelBurnPercentageTarget = getPercentage(downwardsSpeed);
            } else { // balance
                Player.PlayerIronSteel.SteelBurnPercentageTarget = getPercentage(0);
            }

            //Debug.Log("Wanted: " + ((rb.mass + Player.PlayerIronSteel.Mass) * (1 + speed) * -Physics.gravity.y));
            //Debug.Log("Have  : "  + Player.PlayerIronSteel.LastMaximumNetForce.magnitude);
            //Debug.Log("Net force: " + Player.PlayerIronSteel.LastAllomanticForce);
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
