using UnityEngine;

public class Marker_CoinSet : MonoBehaviour
{
    [SerializeField] [Range(1, 25)] int coinAmount = 3;
    [SerializeField] float coinSpacing = 1f;

    enum CoinDir { Horizontal, Vertical };

    [SerializeField] CoinDir coinDirection = CoinDir.Horizontal;

    
    [Header("Gizmo")]
    [SerializeField] bool drawGizmos = true;
    float coinRadius = 0.5f;

    void Awake()
    {
        drawGizmos = false;
    }

    public Vector3[] GetCoinSpawnPositions()
    {
        Vector3[] coins = new Vector3[coinAmount];
        float width = (coinRadius * 2f * coinAmount) + (coinSpacing * (coinAmount - 1));
        Vector3 dir = coinDirection == CoinDir.Horizontal ? transform.right : transform.up;

        int i = 0;
        for(float x = coinRadius; x < width; x += (coinRadius*2f) + coinSpacing)
        {
            coins[i] = transform.position + dir *( x - width/2f);
            i++;
        }

        return coins;
    }


    void OnDrawGizmos()
    {
        if(!drawGizmos) return;

        float width = (coinRadius * 2f * coinAmount) + (coinSpacing * (coinAmount - 1));
        float diameter = coinRadius * 2f;
        Vector3 dir = coinDirection == CoinDir.Horizontal ? transform.right : transform.up;

        Gizmos.color = Color.yellow;

        for(float x = coinRadius; x < width; x += (coinRadius*2f) + coinSpacing)
        {
            Vector3 position = transform.position + dir *( x - width/2f);
            Gizmos.DrawIcon(position, "icon_coin.png");
            Gizmos.DrawWireCube(position, new Vector3(diameter, diameter, 0f));
        }

        // Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0f));
    }
}
