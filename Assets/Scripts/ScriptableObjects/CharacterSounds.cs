using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSounds", menuName = "Characters/Character Sounds")]
public class CharacterSounds : ScriptableObject
{
    public AudioClip jumpSound;
    public AudioClip rollSound;

    //Cancelled feature.
    // public AudioClip brakeSound;
    // public AudioClip deathSound;
    // public AudioClip loseRingSound;
    // public AudioClip[] footStepSounds;
}
