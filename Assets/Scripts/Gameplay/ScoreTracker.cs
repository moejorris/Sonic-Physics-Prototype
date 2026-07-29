using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
    //NOT a GameManager- This does not persist between scenes.
    public static ScoreTracker Instance { get; private set;}

    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;        
        }
    }


    public int coins = 0;

    public void AddCoin(int amount = 1)
    {
        coins += amount;
    }

    public void LoseCoins(int amount)
    {
        if(amount == 0)
        {
            coins = 0;
        }
        else
        {
            coins -= amount;        
        }
    }
}
