using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinPouch : MonoBehaviour {

    [SerializeField]
    private Coin coinPrefab;

    private AllomanticIronSteel parentIronSteel;
    private Rigidbody parentRigidbody;
    private int coinCount;

	// Use this for initialization
	void Start () {
        parentIronSteel = GetComponentInParent<AllomanticIronSteel>();
        parentRigidbody = GetComponentInParent<Rigidbody>();

        coinCount = 50;
        UpdateUI();
    }

    public void AddCoin(Coin coin) {
        if (coin.Allomancer || parentIronSteel.IsTarget(coin))
            parentIronSteel.RemoveTarget(coin, true, true);
        Destroy(coin.gameObject);
        coinCount++;
        UpdateUI();
}

    public Coin WithdrawCoinToHand() {
        if (coinCount > 0) {
            coinCount--;
            UpdateUI();
            Coin coin = Instantiate(coinPrefab, transform.position, transform.rotation);
            coin.GetComponent<Rigidbody>().velocity = parentRigidbody.velocity;
            return coin;
        }
        return null;
    }

    public Coin SpawnCoin(Vector3 position) {
        if (coinCount > 0) {
            coinCount--;
            UpdateUI();
            Coin coin = Instantiate(coinPrefab, position, transform.rotation);
            coin.GetComponent<Rigidbody>().velocity = parentRigidbody.velocity;
            return coin;
        }
        return null;
    }

    private void UpdateUI() {
        HUD.CoinCountText = coinCount.ToString();
    }

    public void Clear() {
        coinCount = 50;
    }
}
