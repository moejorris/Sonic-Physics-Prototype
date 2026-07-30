using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Character Animation", menuName = "Characters/Genesis Sprite Animations")]
public class GenesisSpriteAnimations : ScriptableObject
{
    
    
    [Header("Standing")]
    public CharacterSprite[] idle;
    public CharacterSprite[] balancing;
    public CharacterSprite[] lookUp;
    public CharacterSprite[] crouch;

    [Header("Moving")]
    public CharacterSprite[] walking;
    public CharacterSprite[] running;
    public CharacterSprite[] dashing;
    public CharacterSprite[] pushing;

    public enum BrakeStyle {Sonic1_CD, Sonic2_3_Knuckles};

    public BrakeStyle brakingStyle = BrakeStyle.Sonic1_CD;
    public CharacterSprite[] braking;

    [Header("Ball")]
    public BallSprite[] ball;
    public CharacterSprite[] spindash;
    
}

[Serializable]
public class CharacterSprite
{
    [SerializeField] string animName; //this field is purely for the user to be able to identify faster what this set is, rather than having to guess.
    public bool shouldLoop = true;
    public int loopTimes = 0; //leave 0 to loop infinitely
    public bool variableByGroundSpeed = false;
    public float subImage_duration;
    public Sprite[] sprites;
}

[Serializable]
public class BallSprite : CharacterSprite
{
    public Sprite ballSprite;
}