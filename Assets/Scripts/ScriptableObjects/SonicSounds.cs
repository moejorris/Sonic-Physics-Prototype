using UnityEngine;

[CreateAssetMenu(fileName = "SonicSounds", menuName = "Scriptable Objects/SonicSounds")]
public class SonicSounds : ScriptableObject
{
    public AudioClip jumpSound;
    public AudioClip brakeSound;
    public AudioClip rollSound;
    public AudioClip deathSound;
    public AudioClip loseRingSound;
    public AudioClip footStepSound;
}
