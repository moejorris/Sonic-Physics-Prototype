using UnityEngine;

[CreateAssetMenu(fileName = "Movement Stats", menuName = "Player Characters/Movement Stats", order = 1)]
public class MovementStats : ScriptableObject
{
    [Header("Collision Detection")]
    [Tooltip("Anything with a collider on these layers will be treated as solid for the player sensors.")]
    public LayerMask collisionLayerMask = 1 << 3;

    //Values straight from the physics guide, in original, pixel based units.

    #region  Sizes
    [Header("Sizes")]
    public float width_radius = 9.5f;
    public float height_radius = 19.5f;
    public float push_radius = 10.5f;
    [Tooltip("These size values will be used anytime the player is in ball form, not just rolling.")]
    public float roll_width_radius = 7.5f;
    public float roll_height_radius = 14.5f;    
    #endregion


    #region Normal State
    [Header("Normal State")]
    public float max_jump_force = 6f + 128f/256f;
    public float min_jump_force = 4f;
    public float acceleration_speed = 12f/256f;
    public float decceleration_speed = 128f/256f;
    public float friction_speed = 12f/256f; //same as accel speed
    public float top_speed = 6f;
    public float dash_speed = 10f; //when max speed animation will play if one is used
    public float slope_factor = 32f/256f;
    public float slope_factor_standing_threshold = 13f/256f; //S3 only applies slope factor if it is greater than or equal to this number, regardless of ground speed. The previous (S1, S2) only apply slope factor if ground speed is non zero.
    public float max_crouch_speed = 1f; //The player can crouch if going slower than this
    public float turn_around_speed = 128/256f; //when the player has changed directions by deccelerating, their ground speed is set to this
    public float slipping_speed = 2.5f; //if the player is going slower than this while on a slope (within a specific angle range) the player will slip and activate the control lock.
    public float slipping_s3_repel_speed = 0.5f;
    #endregion


    #region Air State
    [Header("Air State")]
    public float gravity_force = 56f/256f;
    public float gravity_max_fall_speed = 16f;
    public float air_acceleration_speed = 24f/256f;
    public float ground_angle_return_speed = 2.8125f; //the max delta at which the ground angle will rotate back towards 0 while airborne.
    #endregion
    
    #region Roll Substate (part of the grounded state)
    [Header("Roll State")]
    public float roll_activation_speed = 1f;
    public float roll_deactivation_speed = 128f/256f;
    public float roll_friction_speed = 6f/256f; //half of normal friction speed
    public float roll_deceleration_speed = 32f/256f;
    public float roll_turn_around_threshold = 38/256f; //if the player is going slower than this after decceleration is applied, their speed will be set to turn_around_speed.
    public float roll_max_x_speed = 16f;
    public float roll_min_ground_speed = 2f;
    
    //Rolling has 2 different slope factors, allowing the player to speed up significantly faster going down than they slow down going up hill.
    public float roll_slope_factor_up = 20f/256f; 
    public float roll_slope_factor_down = 80f/256f;
    #endregion

    #region Hurt State
    [Header("Hurt State")]
    public float hurt_x_force = 2f;
    public float hurt_y_force = 4f;
    public float hurt_gravity_force = 48f/256f;
    #endregion
    
    #region Speed Caps (values of zero are treated as no cap)
    [Header("Speed Caps\n\nValue of 0 = No Cap")]
    public float max_speed_x = 1f;
    public float max_speed_y = 0f;
    public float max_speed_ground = 0f;
    
    #endregion
}
