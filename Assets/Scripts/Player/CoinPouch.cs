using UnityEngine;

public class CoinPouch : MonoBehaviour {

    private const int startingCoinCount = 50;

    [SerializeField]
    private Coin coinPrefab;
    
    private Rigidbody parentRigidbody;
    public int Count { get; private set; }

	// Use this for initialization
	void Start () {
        parentRigidbody = GetComponentInParent<Rigidbody>();

        Count = startingCoinCount;
        UpdateUI();
    }

    public void AddCoin(Coin coin) {
        Player.PlayerIronSteel.RemoveTarget(coin, true);
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
        Count = startingCoinCount;
    }
}
