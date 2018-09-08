using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinPouch : MonoBehaviour {

    [SerializeField]
    private Coin coinPrefab;
    
    private Rigidbody parentRigidbody;
    public int Count { get; private set; }

	// Use this for initialization
	void Start () {
        parentRigidbody = GetComponentInParent<Rigidbody>();

        Count = 50;
        UpdateUI();
    }

    public void AddCoin(Coin coin) {
        if (coin.Allomancer == Player.PlayerIronSteel)
            Player.PlayerIronSteel.RemoveTarget(coin, true, true);
        Destroy(coin.gameObject);
        Count++;
        UpdateUI();
    }

    public Coin RemoveCoin(Vector3 spawnPosition) {
        if (Count > 0) {
            Count--;
            UpdateUI();
            Coin coin = Instantiate(coinPrefab, spawnPosition, transform.rotation);
            coin.GetComponent<Rigidbody>().velocity = parentRigidbody.velocity;
            return coin;
        }
        return null;
    }

    private void UpdateUI() {
        HUD.CoinCountText = Count.ToString();
    }

    public void Clear() {
        Count = 50;
    }
}
