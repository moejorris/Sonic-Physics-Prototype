using UnityEngine;

public class Object_Coin : MonoBehaviour
{
    Collider2D coll;

    void Awake()
    {
        coll = GetComponent<Collider2D>();
    }
    void FixedUpdate()
    {
        if(coll.bounds.Intersects(PlayerMovement.Player.bounds))
        {
            OnPlayerTouched();
        }
    }

    void OnPlayerTouched()
    {
        if(transform.childCount > 0)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(0).transform.SetParent(null);
        }

        Destroy(gameObject);
    }
}
