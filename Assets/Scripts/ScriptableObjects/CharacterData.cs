using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Characters/New Character", order = 2)]
public class CharacterData : ScriptableObject
{
    public PlayerMovementStats playerMovementStats;
    public GenesisSpriteAnimations spriteAnimations;
    public enum GroundRotationStyle
    {
        Genesis, //45 degree multiples
        Mania, //smooth rotation, no angle snapping.
    };

    public GroundRotationStyle groundRotationStyle = GroundRotationStyle.Genesis;

    public List<ScriptableObject> specialAbilities = new List<ScriptableObject>();
    public CharacterSounds sfx;
}
