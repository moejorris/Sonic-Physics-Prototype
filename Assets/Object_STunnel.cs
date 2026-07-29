using UnityEngine;

public class Object_STunnel : MonoBehaviour
{
    BoxCollider2D coll;
    PlayerMovement playerMovement;
    void Start()
    {
        playerMovement = PlayerMovement.Player;
        coll = GetComponent<BoxCollider2D>();
    }

    void FixedUpdate()
    {
        if(coll == null || playerMovement == null) return;

        if(coll.bounds.Intersects(playerMovement.bounds) && playerMovement.isGrounded())
        {
            playerMovement.ForceStateChange(PlayerMovement.PlayerState.Rolling);
        }
    }
}
