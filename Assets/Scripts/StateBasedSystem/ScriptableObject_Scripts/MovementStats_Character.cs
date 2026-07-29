using UnityEngine;
using static UnitConversion;

[CreateAssetMenu(fileName = "Character Movement Stats", menuName = "Player Characters/Character Movement Stats", order = 1)]
public class MovementStats_Character : ScriptableObject
{
    //Values that differ between characters and super forms.
    //All other values are determined by the physics system (eg gravity, friction) and do not change between characters.
    //However, some factors will modify these values and physics values (underwater affects acceleration and gravity, for example).
    [Header("Ground")]
    public float acceleration = SubpixelToFloat(12);
    public float deceleration = SubpixelToFloat(128);
    public float top_speed = 6f;
    
    [Header("Jumping")]
    public float min_jump_force = 4f;
    public float max_jump_force = 6.5f; //6.5f for Sonic/Tails, 6f for Knuckles.
    
    [Header("Air")]
    public float air_acceleration = SubpixelToFloat(24);
}
