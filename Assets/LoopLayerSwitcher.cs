using UnityEngine;

public class Object_LoopLayerSwitcher : MonoBehaviour
{
    BoxCollider2D coll;
    [SerializeField] EdgeCollider2D rightHalf;
    [SerializeField] EdgeCollider2D leftHalf;

    [SerializeField] float boundsX;

    bool wasColliding = false;

    void Awake()
    {
        coll = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        bool isColliding = coll.bounds.Intersects(PlayerMovement.Player.bounds);

        if(isColliding != wasColliding)
        {
            if(isColliding)
            {
                EnterSensor();
            }
            else
            {
                ExitSensor();
            }
        }
        else
        {
            if(isColliding)
            {
                StaySensor();
            }
            else
            {
                NotInSensor();
            }
        }

        wasColliding = isColliding;
    }

    void NotInSensor()
    {
        if(PlayerMovement.Player.Position.x < transform.position.x + boundsX/2f && PlayerMovement.Player.Position.x > transform.position.x - boundsX/2f)
        {
            return;
        }
        bool rightSideActive = PlayerMovement.Player.Position.x < transform.position.x;


        rightHalf.enabled = rightSideActive;
        leftHalf.enabled = !rightSideActive;
    }

    void EnterSensor()
    {
    }
    
    void ExitSensor()
    {
        bool rightSideActive = PlayerMovement.Player.GroundSpeed < 0;

        rightHalf.enabled = rightSideActive;
        leftHalf.enabled = !rightSideActive;
    }

    void StaySensor()
    {
        rightHalf.enabled = true;
        leftHalf.enabled = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Vector3 size = new Vector3(boundsX, 12f, 0.01f);
        Gizmos.DrawWireCube(transform.parent.position + (size.y * 0.5f * Vector3.up), size);
    }
}
