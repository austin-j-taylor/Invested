using UnityEngine;

public class CoinPouch : MonoBehaviour {

    public const int startingCoinCount = 50;
    public readonly Vector3 coinThrowSpeed = new Vector3(0, 0, 5);

    [SerializeField]
    private Coin coinPrefab;
    
    private Rigidbody parentRigidbody;
    public int Count { get; private set; }

	// Use this for initialization
	void Start () {
        parentRigidbody = GetComponentInParent<Rigidbody>();
        Count = startingCoinCount;
    }

    public void AddCoin(Coin coin) {
        Player.PlayerIronSteel.RemoveTarget(coin, true);
        Destroy(coin.gameObject);
        Count++;
    }

    public Coin RemoveCoin(Vector3 spawnPosition) {
        if (Count > 0) {
            Count--;
            Coin coin = Instantiate(coinPrefab, spawnPosition, transform.rotation);
            coin.GetComponent<Rigidbody>().velocity = parentRigidbody.velocity + transform.rotation * coinThrowSpeed;
            return coin;
        }
        return null;
    }

    public void Clear() {
        Count = startingCoinCount;
    }
}
