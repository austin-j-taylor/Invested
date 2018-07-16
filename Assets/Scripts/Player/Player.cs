using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    
    private const float coinCooldown = 0.1f;
    private readonly Vector3 feet = Vector3.zero;

    private Animator animator;
    private Hand hand;
    public AllomanticIronSteel IronSteel { get; private set; }
    public CoinPouch Pouch { get; private set; }

    private float lastCoinThrowTime = 0;

    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();
        IronSteel = GetComponent<AllomanticIronSteel>();
        hand = GetComponentInChildren<Hand>();
        Pouch = GetComponentInChildren<CoinPouch>();

    }
	
	// Update is called once per frame
	void Update () {
        // On pressing COIN button
        if (Keybinds.WithdrawCoinDown() && lastCoinThrowTime + coinCooldown < Time.time) {
            lastCoinThrowTime = Time.time;
            // If jump button had also been held, throw coin downward and target it.
            if (Keybinds.Jump()) {
                Coin coin = Pouch.SpawnCoin(transform.position + feet);
                IronSteel.AddTarget(coin, false);

            } else { // If only pressing the COIN button, draw a coin into hand
                Coin coin = Pouch.WithdrawCoinToHand();
                IronSteel.AddTarget(coin, false);
            }
        }
	}
}
