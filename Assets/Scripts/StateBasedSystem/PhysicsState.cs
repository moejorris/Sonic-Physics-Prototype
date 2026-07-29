using UnityEngine;
public class PhysicsState
{
    protected PhysicsState(){} //avoids having to create an empty constructor in every state child class :D
    protected PlayerObject player; //ref to the player
    protected PhysicsStateManager manager;
    
    //Position Helpers
    protected Vector2 position 
    {
        get => player.position;
        set => player.position = value;
    }
    protected float x_position
    {
        get => player.position.x;
        set => player.position.x = value;
    }
    protected float y_position
    {
        get => player.position.y;
        set => player.position.y = value;
    }

    //Velocity Helpers
    protected Vector2 velocity
    {
        get => player.velocity;
        set => player.velocity = value;
    }
    protected float x_speed //following physics guide naming convention for those familiar. velocity.x and velocity.y are referred to as X Speed and Y Speed respectively.
    {
        get => player.velocity.x;
        set => player.velocity.x = value;
    }
    protected float y_speed
    {
        get => player.velocity.y;
        set => player.velocity.y = value;
    }

    //Ground Helpers
    protected int controlLock
    {
        get => player.controlLock;
        set => player.controlLock = value;
    }
    protected float groundAngle
    {
        get => player.groundAngle;
        set => player.groundAngle = value;
    }
    protected float groundSpeed
    {
        get => player.groundSpeed;
        set => player.groundSpeed = value;
    }

    protected float absGroundSpeed => Mathf.Abs(groundSpeed);

    //Misc. Helpers
    protected MovementStats stats => player.stats;
    protected InputManager input => InputManager.Instance;
    protected PlayerCollisionResolution collision => player.collision;

    public void AssignReferences(PlayerObject playerPhysics, PhysicsStateManager stateManager)
    {
        player = playerPhysics;
        manager = stateManager;
    }

    public virtual void Enter()
    {
        
    }

    public virtual void Exit()
    {
        
    }

    public virtual void Update()
    {
        
    }
}
