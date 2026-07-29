using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Character", menuName = "Sonic Characters/New Character", order = 2)]
public class SonicCharacter : ScriptableObject
{
    public PlayerMovementStats playerMovementStats;
    public GenesisSonicAnimations genesisAnimations;
    public enum GroundRotationStyle
    {
        Genesis,
        Mania,
        Modern
    };

    public GroundRotationStyle groundRotationStyle = GroundRotationStyle.Genesis;

    public List<ScriptableObject> specialAbilities = new List<ScriptableObject>();
}
