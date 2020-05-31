using UnityEngine;

public class CoinPouch : MonoBehaviour {

    public const int startingCoinCount = 50;

    public int Count { get; private set; }
    public bool IsEmpty { get { return Count == 0; } }

    void Start() {
        Count = startingCoinCount;
    }

    public void AddCoin(Coin coin) {
        Destroy(coin.gameObject);
        Count++;
    }

    // Removes a coin from the pouch, instantiating it
    public Coin RemoveCoin(Vector3 spawnPosition) {
        if (Count > 0) {
            Count--;
            Coin coin = Instantiate(GameManager.Prefab_Coin, spawnPosition, transform.rotation);
            return coin;
        }
        return null;
    }

    public void Clear() {
        Count = 0;
    }

    public void Fill() {
        Count = startingCoinCount;
    }
}
