using UnityEngine;

public class ObjectSpawner_Coin : MonoBehaviour
{
    [SerializeField] GameObject coinPrefab;
    void Awake()
    {
        if(coinPrefab == null)
        {
            enabled = false;
            return;
        }

        Marker_CoinSet[] coinSets = FindObjectsByType<Marker_CoinSet>(FindObjectsSortMode.None);

        foreach(Marker_CoinSet coinSet in coinSets)
        {
            foreach(Vector3 position in coinSet.GetCoinSpawnPositions())
            {
                Instantiate(coinPrefab, position, Quaternion.identity, coinSet.transform);
            }
        }
    }
}
