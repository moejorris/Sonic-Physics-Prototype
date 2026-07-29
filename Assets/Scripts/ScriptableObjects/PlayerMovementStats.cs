using UnityEngine;

[CreateAssetMenu(fileName = "Player Movement Stats", menuName = "Characters/PlayerMovementStats", order = 1)]
public class PlayerMovementStats : ScriptableObject
{
    //size vars
    public float standingWidth = 1.1875f;
    public float standingHeight = 2.4375f;
    public float ballWidth = 0.9375f;
    public float ballHeight = 1.8125f;
    public float pushRadius = 0.625f;

    //forces and speed values: values taken from sonic retro's physics guide and divided by 16 (16 pixels / unit)
    public float jump_force = 0.40625f;
    public float acceleration_speed = 0.0029296875f;
    public float decceleration_speed = 0.03125f;
    public float friction_speed = 0.0029296875f; //same as accel speed
    public float roll_activation_speed = 0.03125f;
    public float roll_friction_speed = 0.00146484375f; //same as accel speed and friction, just half
    public float roll_deceleration_speed = 0.0078125f;
    public float top_speed = 0.375f;
    public float dash_speed = 0.625f; //when max speed animation will play if one is used
    public float gravity_force = 0.013671875f;
    public float air_acceleration_speed = 0.005859375f;

    //ground detection
    public LayerMask groundLayerMask = 1 << 3;

    //slope factors
    public float slope_factor_normal = 0.0078125f;
    public float slope_factor_rollup = 0.0048828125f;
    public float slope_factor_rolldown = 0.01953125f;
    public float slope_factor_standing_threshold = 0.00317383f;

    //hurt forces
    public float hurt_x_force = 0.125f;
    public float hurt_y_force = 0.25f;
    public float hurt_gravity_force = 0.01171875f;

    //Limits
    public float max_speed_x = 1f;
    public float max_speed_y = 2f;
    public float max_speed_ground = 2f;

}
