using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Object_Spring : MonoBehaviour
{
    [SerializeField] float launchHeight = 10f;
    BoxCollider2D coll;
    bool playerInside = false;
    void Awake()
    {
        coll = GetComponent<BoxCollider2D>();
    }

    void FixedUpdate()
    {
        UpdateCollision();
    }

    void UpdateCollision()
    {
        if(PlayerMovement.Player == null || PlayerMovement.Player.PrimaryGroundSensor == null)
        {
            playerInside = false;
            return;
        }
        SensorHit playerGroundSensor = PlayerMovement.Player.PrimaryGroundSensor;
        bool playerOnTop = playerGroundSensor.hit && playerGroundSensor.collider == coll && playerGroundSensor.rawNormal == (Vector2)transform.up && playerGroundSensor.distance <= 0;

        bool newCollision = playerOnTop; // || PlayerMovement.Player.bounds.Intersects(coll.bounds);

        if(!playerInside && newCollision)
        {
            OnPlayerEnter();
        }
        else if(playerInside && !newCollision)
        {
            OnPlayerExit();
        }
        else if(playerInside && newCollision)
        {
            OnPlayerStay();
        }

        playerInside = newCollision;
    }

    void OnPlayerEnter()
    {
        LaunchPlayer();
    }

    void OnPlayerStay()
    {
        if(PlayerMovement.Player.Velocity.y == 0)
        {
            LaunchPlayer();
        }

        playerInside = false;
    }

    void OnPlayerExit()
    {
        
    }

    void LaunchPlayer()
    {
        Vector2 launchVector = transform.up * launchHeight;

        //if the launch direction has a positive y component, we must adjust it to combat gravity.
        if(transform.up.y > 0f)
        {
            launchVector.y = Mathf.Sqrt(2f * Mathf.Abs(PlayerMovement.Player.movementStats.gravity_force) * launchHeight);
        }
        
        PlayerMovement.Player.SpringPlayer(launchVector);
    }

    void OnDrawGizmosSelected()
    {
        if(coll == null)
        {
            coll = GetComponent<BoxCollider2D>();
        }

        Gizmos.color = (Color.red + Color.white) / 2f;
        Vector3 colliderSize = new Vector3(coll.size.x * transform.localScale.x, coll.size.y * transform.localScale.y, 0.1f);

        Gizmos.DrawRay(transform.position - colliderSize.x / 2f * transform.right, transform.up * launchHeight);
        Gizmos.DrawRay(transform.position + colliderSize.x / 2f * transform.right, transform.up * launchHeight);

        // Gizmos.DrawWireCube(boxCenter, gizmoSize);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        if(coll == null)
        {
            coll = GetComponent<BoxCollider2D>();
        }

        //draw activation buffer

        Bounds triggerBounds = coll.bounds;
        triggerBounds.center += transform.up * 0.1f;

        Vector2 widthOffset = (Vector2) transform.right * triggerBounds.extents;
        Vector2 heightOffset = (Vector2) transform.up * triggerBounds.extents;


        Gizmos.DrawLine((Vector2)triggerBounds.center - widthOffset + heightOffset, (Vector2)triggerBounds.center + widthOffset + heightOffset);
    }
}
