using UnityEngine;
using System.Collections;

/*
 * An elevator that travels up and down a shaft as the Sphere controls it.
 * When On, the elevator reduces its weight with fIron.
 */
public class Elevator : Interfaceable {

    protected override IEnumerator Interaction() {
        ReceivedReply = false;

        HUD.ConsoleController.Log(" > ");
        yield return new WaitForSeconds(2);
        HUD.ConsoleController.TypeInLine("Please give me access to this elevator, kind soul.", this);
        while(!ReceivedReply) {
            yield return null;
        }
        ReceivedReply = false;
        yield return new WaitForSeconds(2);
        HUD.ConsoleController.LogLine("_CONTROLLER_INIT()");
        yield return new WaitForSeconds(1.4f);
        HUD.ConsoleController.LogLine("_SSSH_VALIDATE()");
        yield return new WaitForSeconds(.6f);
        HUD.ConsoleController.LogLine("[CRITICAL] Your SSSH certificate is out of date by 212 months. Please contact your spiritual service provider.");
        yield return new WaitForSeconds(2.2f);
        HUD.ConsoleController.LogLine("_AI_INIT()");
        yield return new WaitForSeconds(2f);
        HUD.ConsoleController.LogLine("_F_IRON_INIT()");
        yield return new WaitForSeconds(2.5f);
        HUD.ConsoleController.LogLine("_START_COROUTINE_F_IRON_STORE()");
        yield return new WaitForSeconds(.5f);
        HUD.ConsoleController.LogLine("_DOORS_INIT()");

        HUD.ConsoleController.Log(" > ");
        yield return new WaitForSeconds(3);
        HUD.ConsoleController.TypeInLine("Please unlock the doors to the upper facilities, kind soul.", this);
        while (!ReceivedReply) {
            yield return null;
        }
        ReceivedReply = false;
        yield return new WaitForSeconds(.25f);
        HUD.ConsoleController.LogLine("Unlocking doors to upper facilities ...");
        yield return new WaitForSeconds(3);
        HUD.ConsoleController.LogLine("Door 0 unlocked.");
        yield return new WaitForSeconds(.1f);
        HUD.ConsoleController.LogLine("[ERROR] No response received from door 1.");
        yield return new WaitForSeconds(.1f);
        HUD.ConsoleController.LogLine("[ERROR] No response received from door 2.");
        yield return new WaitForSeconds(.1f);
        HUD.ConsoleController.LogLine("[ERROR] No response received from door 3.");
        yield return new WaitForSeconds(.1f);
        HUD.ConsoleController.LogLine("[ERROR] No response received from door 4.");
        yield return new WaitForSeconds(2);

        HUD.ConsoleController.Log(" > ");
        yield return new WaitForSeconds(1);
        HUD.ConsoleController.TypeInLine("Thank you for your time, kind soul. I will come for you will I learn what happened here.", this);
        while (!ReceivedReply) {
            yield return null;
        }
        ReceivedReply = false;

        Player.CanControl = true;
        yield return new WaitForSeconds(5);
        HUD.MessageOverlayCinematic.FadeIn("Press " + TextCodes.Space + " to exit the console.");
    }
}
