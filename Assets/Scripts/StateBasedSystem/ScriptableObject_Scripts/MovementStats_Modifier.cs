using UnityEngine;

[CreateAssetMenu(fileName = "MovementStats_Modifier", menuName = "Player Characters/Stats Modifier")]
public class MovementStats_Modifier : ScriptableObject
{
    public float multiplier = 1f;

    [Header("Manual Adjustments" + "\n\nThese values override the stats values.\nSet them to 0 to use the multiplier instead.\n")]
    public float gravity = 56f/256f;
    public float hurt_gravity = 48f/256f;
}
