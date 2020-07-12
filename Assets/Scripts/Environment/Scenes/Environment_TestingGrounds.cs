using UnityEngine;
using System.Collections;

public class Environment_TestingGrounds : EnvironmentCinematic {

    //private bool done;
    [SerializeField]
    private GameObject coinChest = null;

    private bool triggered = false;

    void Start() {
        if (!Player.CanThrowCoins)
            coinChest.SetActive(false);
    }

    protected override IEnumerator Trigger0() {
        if (!Player.PlayerIronSteel.SteelReserve.IsEnabled && !triggered) {
            //challengeToQuit.LeaveChallenge();

            //GameManager.ConversationManager.StartConversation("DIFFICULT");
            //while (HUD.ConversationHUDController.IsOpen)
            //    yield return null;
            yield return new WaitForSeconds(10);
            HUD.MessageOverlayCinematic.FadeInFor("If it's hard to navigate with your current skillset,\nconsider returning when more " + TextCodes.Red("powers") + " are remembered.", 7);
            triggered = true;
            yield break;
        } else Debug.Log("already done");
    }
}
