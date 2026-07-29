using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSounds", menuName = "Characters/Character Sounds")]
public class CharacterSounds : ScriptableObject
{
    public AudioClip jumpSound;
    public AudioClip brakeSound;
    public AudioClip rollSound;
    public AudioClip deathSound;
    public AudioClip loseRingSound;
    public AudioClip footStepSound;
}
