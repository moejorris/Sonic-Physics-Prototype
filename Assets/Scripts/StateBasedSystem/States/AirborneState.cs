using UnityEngine;

public class AirborneState : PhysicsState
{
    enum SubState
    {
        Normal,
        Jumping,
        Springing
    }
    public override void Enter()
    {
        
    }

    public override void Exit()
    {
        
    }

    public override void Update()
    {
        VariableJumpCheck();

        SuperTransformCheck();

        UpdateXSpeed();

        ApplyAirDrag();

        position += velocity;

        ApplyGravity();

        //Reduce underwater gravity

        RotateGroundAngle();

        ResolveAirborneCollisions();
    }

    void VariableJumpCheck()
    {
        if(player.isJumping && y_speed > stats.min_jump_force && y_speed < stats.max_jump_force && !input.jumpButton)
        {
            y_speed = stats.min_jump_force;
        }
    }

    void SuperTransformCheck()
    {
        
    }

    void UpdateXSpeed()
    {
        bool canAccelerate = Mathf.Abs(x_speed) < stats.top_speed || Mathf.Sign(input.moveInputDir.x) != Mathf.Sign(x_speed);
        bool isInputting = input.moveInputDir.x != 0f;

        if(canAccelerate && isInputting)
        {
            x_speed += Mathf.Sign(input.moveInputDir.x) * stats.air_acceleration_speed;
            if(Mathf.Abs(x_speed) > stats.top_speed)
            {
                x_speed = stats.top_speed * Mathf.Sign(x_speed);
            }
        }
    }

    void ApplyAirDrag()
    {
        
    }

    void ApplyGravity()
    {
        y_speed = Mathf.Max(y_speed - stats.gravity_force, -stats.gravity_max_fall_speed);
    }

    void RotateGroundAngle()
    {
        groundAngle = Mathf.MoveTowardsAngle(groundAngle, 0f, stats.ground_angle_return_speed);
    }

    void ResolveAirborneCollisions()
    {
        bool mostlyHorizontal = Mathf.Abs(x_speed) >= Mathf.Abs(y_speed);

        if(mostlyHorizontal)
        {
            collision.ResolvePushCollisions();
            collision.ResolveGroundCollisions();
            collision.ResolveCeilingCollisions();
        }
        else
        {
            collision.ResolvePushCollisions();

            if(y_speed >= 0f)
            {
                collision.ResolveCeilingCollisions();
            }
            else
            {
                collision.ResolveGroundCollisions();
            }
        }
    }

    public void VelocityToGroundSpeed(bool ceilingMode = false)
    {
        float symmetricalAngle = groundAngle > 180f ? Mathf.Abs(groundAngle - 360f) : groundAngle;

        bool flatRange = symmetricalAngle <= 23f;
        bool slopeRange = symmetricalAngle <= 45f;

        bool mostlyHorizontal = Mathf.Abs(x_speed) >= Mathf.Abs(y_speed);

        if(flatRange || mostlyHorizontal)
        {
            groundSpeed = x_speed;
        }
        else
        {
            float newGroundSpeed = y_speed * Mathf.Sign(Mathf.Sin(groundAngle * Mathf.Deg2Rad));
            
            if(slopeRange)
            {
                groundSpeed = newGroundSpeed * 0.5f;
            }
            else groundSpeed = newGroundSpeed;

            Debug.Log("ground speed derived from y_speed of " + y_speed + " converted to: " + newGroundSpeed);
        }
    }
}
