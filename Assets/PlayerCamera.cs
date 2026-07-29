using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera instance;
    PlayerMovement player;
    float horizontalRadius = 16f/2f/16f;
    float verticalAirRadius = 32f/16f;

    float yOffset = -1f; //in Sonic 1, sonic's center is always 16 pixels above the center of the screen.
    [SerializeField] float lookDownOffset = 104f/16f;
    [SerializeField] float lookUpOffset = 88f/16f;
    // [SerializeField] float lookSpeed = 2f/16f;
    float lookOffsetY = 0f;
    float zOffset = 10f;
    Camera cam;

    float normSpeedCap = 6f/16f;
    float fastSpeedCap = 16f/16f;
    float abnormalSpeedCap = 2f/16f;

    float yInputTimer = 0f;
    // float speedCapThreshold = 8f/16f;

    Vector2 currentPos;

    void Start()
    {
        cam = GetComponent<Camera>();
        player = PlayerMovement.Player;

        //fastSpeedCap = Mathf.Max(PlayerMovement.Player.movementStats.max_speed_ground, PlayerMovement.Player.movementStats.max_speed_x, PlayerMovement.Player.movementStats.max_speed_y);

        if(transform.parent != null) //detach from any parent object
        {
            transform.parent = null;
        }
    }

    void Awake()
    {
        instance = this;
        currentPos = transform.position;
    }

    //TODO: FIX SPEED/Maybe just lock sonic within the border?????


    void LateUpdate()
    {
        float targetY = player.transform.position.y;
        float targetX = player.transform.position.x;
        float playerSpeed = Mathf.Abs(player.isGrounded() ? player.GroundSpeed : player.Velocity.magnitude);
        float moveSpeed = playerSpeed > normSpeedCap ? fastSpeedCap : normSpeedCap;
        float deltaTime = 1f / Time.fixedDeltaTime * Time.deltaTime; //the speed values were originally for 60fps (1f/60f) so we use this to "undo" that.
        float yInput = InputManager.Instance.moveInputDir.y;

        if(yInput != 0f)
        {
            yInputTimer += Time.deltaTime;
        }
        else yInputTimer = 0f;

        if(player.isGrounded())
        {
            if(player.GroundSpeed == 0 && yInputTimer >= 2f)
            {
                float desiredLookOffsetY = yInput > 0 ? lookUpOffset : -lookDownOffset;
                lookOffsetY = Mathf.MoveTowards(lookOffsetY, desiredLookOffsetY - yOffset, abnormalSpeedCap * deltaTime);
            }
            else
            {
                lookOffsetY = Mathf.MoveTowards(lookOffsetY, 0f, abnormalSpeedCap * deltaTime);
            }

            float diff = Mathf.Abs(targetY - currentPos.y);
            diff = Mathf.Min(diff, moveSpeed * deltaTime);
            currentPos.y = Mathf.MoveTowards(currentPos.y, targetY, diff);
        }
        else
        {
            lookOffsetY = Mathf.MoveTowards(lookOffsetY, 0f, abnormalSpeedCap * deltaTime);

            if(!IsInsideOfCamRangeY(targetY))
            {
                float distFromEdge = Mathf.Abs(targetY - currentPos.y) - verticalAirRadius;

                distFromEdge = Mathf.Min(distFromEdge, moveSpeed * deltaTime);

                currentPos.y = Mathf.MoveTowards(currentPos.y, targetY, distFromEdge);
            }
        }

        if(!IsInsideOfCamRangeX(targetX))
        {
            float distFromEdge = Mathf.Abs(targetX - currentPos.x) - horizontalRadius;

            distFromEdge = Mathf.Min(distFromEdge, moveSpeed * deltaTime);

            currentPos.x = Mathf.MoveTowards(currentPos.x, targetX, distFromEdge);
        }

        // currentPos.x = Mathf.MoveTowards(currentPos.x, targetX, moveSpeed * deltaTime);
        // currentPos.y = Mathf.MoveTowards(currentPos.y, targetY, moveSpeed * deltaTime);

        Vector3 targetPosition = new Vector3(currentPos.x, currentPos.y + yOffset + lookOffsetY, -zOffset);

        transform.position = targetPosition;
    }

    public void WarpCam(Vector2 position)
    {
        currentPos.x = position.x;
        currentPos.y = position.y;
    }

    bool IsInsideOfCamRangeY(float y)
    {
        float camY = currentPos.y;

        if(y > camY + verticalAirRadius || y < camY - verticalAirRadius)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    bool IsInsideOfCamRangeX(float x)
    {
        float camX = transform.position.x;

        if(x > camX + horizontalRadius || x < camX - horizontalRadius)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 camPosition = transform.position - (Vector3.forward * transform.position.z) - (Vector3.up * yOffset);
        Gizmos.color = new Color32(76, 255, 0, 255); //SRPG green "vertical focal point" debug color
        Gizmos.DrawLine(camPosition - Vector3.right * horizontalRadius, camPosition);
        Gizmos.DrawLine(camPosition+ Vector3.right * horizontalRadius, camPosition);

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(camPosition, new Vector3(horizontalRadius * 2f, verticalAirRadius * 2f, 0.1f));

        if(player != null)
        {
            Gizmos.color = IsInsideOfCamRangeY(player.transform.position.y) ? Color.blue : Color.red;
            Gizmos.DrawWireSphere(new Vector2(transform.position.x, player.transform.position.y), 0.1f);
        }

    }
}
