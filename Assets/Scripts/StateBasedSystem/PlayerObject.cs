using UnityEngine;

public class PlayerObject : MonoBehaviour
{
    public PlayerCharacter character;
    public Vector2 position = Vector2.zero;
    public Vector2 halfSize = Vector2.zero;
    public Vector2 velocity = Vector2.zero;
    public float groundAngle = 0f;
    public float groundSpeed = 0f;
    public int controlLock = 0;
    public bool isBall = false;
    public bool isBalancing = false;
    public bool isJumping = false;
    public bool isGrounded => stateManager.currentState is GroundedState;
    public MovementStats stats;
    public PlayerCollisionResolution collision;
    public PhysicsStateManager stateManager;
    SpriteRenderer spriteRenderer;
    Vector2 spawnPoint;

    void Awake()
    {
        position = UnitConversion.ToPixelSpace(transform.position);
        SetSpawnPoint(transform.position);

        if(!character)
        {
            character = ScriptableObject.CreateInstance<PlayerCharacter>();
            character.CreateFallBackReferences();
        }

        if(!stats)
        {
            stats = ScriptableObject.CreateInstance<MovementStats>();
        }
        stateManager = new(this);
        collision = new(this);
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        stateManager.Update();
        // transform.position = UnitConversion.ToWorldSpace(position);
        // transform.eulerAngles = Vector3.forward * groundAngle;
    
        if(spriteRenderer)
        {
            float height_radius = isBall ? stats.roll_height_radius : stats.height_radius;
            spriteRenderer.transform.localPosition = Vector2.down * UnitConversion.ToWorldSpace(height_radius);
        }

        // transform.position = UnitConversion.ToWorldSpace(position);
    }

    void Update()
    {
        float alpha = (Time.time - Time.fixedTime)/Time.fixedDeltaTime;
        transform.position = Vector3.Lerp(UnitConversion.ToWorldSpace(position), UnitConversion.ToWorldSpace(position + velocity), alpha);
        float halfAngle = groundAngle > 180f ? Mathf.Abs(groundAngle - 360f) : groundAngle;
        float visAngle = Mathf.RoundToInt(halfAngle / 45f) * 45f * (groundAngle > 180f ? -1f : 1f); 
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(transform.forward * visAngle), alpha);
        
        if(Input.GetKeyDown(KeyCode.R))
        {
            Respawn();
        }

        if(Input.GetKeyDown(KeyCode.Z))
        {
            groundSpeed += 2f * Mathf.Sign(InputManager.Instance.moveInputDir.x);
        }

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            Time.timeScale = 0.05f;
        }
        else if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            Time.timeScale = 1f;
        }
    }

    public enum ControlLockType
    {
        HorizontalSpring = 16, //locks controls for 16 frames
        Slipping = 30, //locks controls for 30 frames
    }
    public void SetControlLock(ControlLockType lockType)
    {
        controlLock = (int)lockType;
    }

    public void ForceRolling()
    {
        stateManager.ChangeState<GroundedState>();
        (stateManager.currentState as GroundedState)?.StartRoll();
    }

    void Ball_ShiftHeight()
    {
        Vector2 dir = collision != null ? collision.GroundDirection() : Vector2.down;

        float targetHeight = isBall ? stats.height_radius : stats.roll_height_radius;
        float amountToMove = halfSize.y - targetHeight;

        position += dir * amountToMove;
    }

    public void EnterBallForm()
    {
        Ball_ShiftHeight();
        isBall = true;

        halfSize.x = stats.roll_width_radius;
        halfSize.y = stats.roll_height_radius;
    }

    public void ExitBallForm()
    {
        Ball_ShiftHeight();
        isBall = false;

        halfSize.x = stats.width_radius;
        halfSize.y = stats.height_radius;
    }

    public void BecomeGrounded()
    {
        stateManager.ChangeState<GroundedState>();
    }

    public void BecomeAirborne()
    {
        stateManager.ChangeState<AirborneState>();
    }

    public void SetSpawnPoint(Vector2 position, bool convertToPixelSpace = true)
    {
        spawnPoint = convertToPixelSpace ? UnitConversion.ToPixelSpace(position) : position;
    }

    public void Respawn()
    {
        position = spawnPoint;

        groundSpeed = 0f;
        groundAngle = 0f;
        velocity = Vector2.zero;
        BecomeGrounded();
        collision.ResolveGroundCollisions();
        collision.SnapPlayerToFloor();
    }

    void OnDrawGizmos()
    {
        collision?.OnDrawGizmos();
    }
}
